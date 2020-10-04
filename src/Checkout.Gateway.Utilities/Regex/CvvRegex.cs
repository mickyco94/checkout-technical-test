namespace Checkout.Gateway.Utilities.Regex
{
    public class CvvRegex : ICvvRegex
    {
        private const string CvvRegexConstant = "^[0-9]{3,4}$";

        public bool IsMatch(string input) => System.Text.RegularExpressions.Regex.IsMatch(input, CvvRegexConstant);
    }
}