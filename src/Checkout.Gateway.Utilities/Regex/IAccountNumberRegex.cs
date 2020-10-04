namespace Checkout.Gateway.Utilities.Regex
{
    public interface IAccountNumberRegex
    {
        bool IsMatch(string input);
    }

    public class AccountNumberRegex : IAccountNumberRegex
    {
        //(MCO) Domain knowledge gap here, not sure what would qualify as a universal account number checker that applies for accounts outside of the UK also.
        //There seems to be inconsistency and a lack of a global standard, perhaps a way of identifying payment recipient other than sort code/account number is used that is more global
        public bool IsMatch(string input) => true;
    }
}