using System;

namespace Checkout.Gateway.Utilities
{
    public static class DateTimeExtensions
    {
        public static DateTime EndOfMonth(this DateTime dateTime) =>

            new DateTime(
                dateTime.Year,
                dateTime.Month,
                DateTime.DaysInMonth(dateTime.Year, dateTime.Month),
                23,
                59,
                59);
    }
}