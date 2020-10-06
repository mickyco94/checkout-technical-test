using System;

namespace Checkout.Gateway.Utilities
{
    public static class StringExtensions
    {
        public static string Mask(this string input, int count, int start = 0, char mask = 'X')
        {
            if (start > input.Length) throw new ArgumentOutOfRangeException(nameof(start), start, $"start value exceeds {input.Length}");
            if (count > input.Length - start) throw new ArgumentOutOfRangeException(nameof(count), count, $"masked portion extends past the length of the string");

            var firstPart = input.Substring(0, start);
            var lastPart = input.Substring(start + count);
            var middlePart = new string(mask, count);

            return firstPart + middlePart + lastPart;
        }
    }
}