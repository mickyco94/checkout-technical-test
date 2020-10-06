## Async Branch

This branch demonstrates an asynchronous flow for processing payments.

A payment is first created in a pending state, other services are then notified of this and process the payment in the background. Updating the status of the payment when appropriate.

I'm using Mediator's Publish functionality here as a substitute for an event broker. In a real production application I would use something like RabbitMQ, Azure Service Bus etc. and publish the events to that instead of Mediatr.

Using events here allows Merchant's to receive quick responses while the payment is processed in the background. They can then query the state of the payment or better yet be notified via webhook (we could add an event handler for the `PaymentCreatedEvent` and `PaymentRejectedEvent`). Being able to distribute the concerns via events allows individual processes to work without needing to deal with distributed transactions (see: Edge cases section below). It also allows for more scalability as individual workers can process these events.

Events also allow us to perform retries that can help mitigate transient errors without adding to response times to a client using the Gateway API.

Events also act as a log. So if any individual handler fails the source can be traced and the messages replayed once the issue is resolved.

(NOTE: This branch does not have updated tests due to time)

## Assumptions

- Bank API requires an account for the funds to be paid into
- Bank API is subject to transient errors that we should be able to deal with (as any API is)
- Bank API has methods to deal with idempotent requests that safely allow retries
- Infrastructure we would be deploying to is able to export container logs

## Improvements

- Everything currently in the `Checkout.Gateway.Utilities` project could be moved out into a set of internal nuget packages. The functionality in this project is not specific to the domain of the Gateway so would be useful for other parts of the system.
- Currently using a mock document DB. In a real production app this would be substituted for a NoSQL database.
- The Authentication implementation is very basic here, as alluded to by a comment in the codebase. A few considerations for Auth implementations are usually safe signing of keys, ability to revoke a keys access, storing of the signing secret in a keyvault etc. I can go into more detail on this area if you would like.
- The encryption is left as a stub for now without any implementation, usually I would use RSA encryption with a different key per merchant. Similary to auth, encryption is a large area of the system with a consistent pattern established across all services.
- Currently the error responses differ slightly if the failure occurred with in the validator compared to a failure within the handler. I tried overriding the output response of FluentValidation but after an hour of failed attempts I didn't feel like a good use of the limited time I have for this tech test. Usually I would use an internal library I developed here at PushDr that I am more comfortable using.
- Currently the idempotency filter determines whether or not a handler was successful based on the status code that is returned. I think something more reliable like a generic HandlerResponse that is propagated through the context would be better as the StatusCode feels like a flakey source of truth that could open the service up to bugs if a developer working on the handler doesn't have intimate knowledge of the idempotency logic.
- Postman tests could be added so developers can quickly and easily verify changes are non breaking. Ideally these would run in a CI pipeline after deployment to test environment.
- Mock Bank API currently has a few cards that can be used for testing scenarios defined as constants. It would be better if these values were configurable.
- Automapper could be added to improve the codebase, removing the boiler plate from mapping DTOs
- Process rejected payment handler and process successful payment handler could be brought together to a shared dependnecy, this to me would depend on future changes. If every change that is going to affect one must affect another then I would move those behaviours into a shared class. However introducing a dependency unneccessarily can tightly couple things that in the future may diverge, experience tells me that bringing these kind of things together too early hurts in the long run. It is easier to consolidate common behaviour later than it is to break apart tightly coupled behaviour. The two classes have very similar actions right now, but that that might not always be the case in the future.
- Further logging could be added, this would dependent on what is the standard of the team
- Logging could be made configurable so the LogLevel can be changed per environment using environment variables. Currently it is set to DEBUG
- Swagger documentation could be improved to include XML comments

## Edge cases

### Database failure

There is a potential for failure after the request has been to the Bank API. At this point the encryption could fail or the writing of the PaymentRecord to the database. This would leave the system in a difficult state as the Bank will have processed the payment but our system would have no record of that.

Initially I thought to create the payment record in a pending state before making the request to the bank but this doesn't solve the issue. The update of the payment record is still subject to failure and we have the same issue.

Effectively the processing of the payment by the bank and writing that result to the database are one single transaction. We don't want one to happen without the other.

In a real production application I would look for a solution more similar to the `async` branch in this project. Breaking apart the concerns into individual event handlers so we don't deal with two different possible points of failure in a single handler. The event handler can then be fed by a message broker that keeps an event log and allows retries in the case of transient errors (the most likely reason a database write would fail).
