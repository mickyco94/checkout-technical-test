namespace Checkout.Gateway.Utilities.Regex
{
    public interface ISortCodeRegex
    {
        bool IsMatch(string input);
    }

    public class SortCodeRegex : ISortCodeRegex
    {
        //(MCO) Domain knowledge gap here, not sure what would qualify as a universal sort code checker that applies for accounts outside of the UK also.
        //There seems to be inconsistency and a lack of a global standard, perhaps a way of identifying payment recipient other than sort code/account number is used that is more global. Leaving this is as a stub.
        public bool IsMatch(string input) => true;
    }
}