using System;

namespace Checkout.Gateway.Utilities
{
    public class GuidHelper : IGuid
    {
        public Guid NewGuid() => Guid.NewGuid();
    }

    public interface IGuid
    {
        Guid NewGuid();
    }

    public interface IDateTime
    {
        DateTime UtcNow();
    }

    public class DateTimeHelper : IDateTime
    {
        public DateTime UtcNow() => DateTime.UtcNow;
    }
}
