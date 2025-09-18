using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Munk.Utils.Object
{
    public static class StringExtensions
    {
        public static double ToDoubleFromUS(this string value)
        {
            if (value == null)
                return 0;

            double newValue = 0;

            if (Double.TryParse(value, NumberStyles.Float, new CultureInfo("en-US"), out newValue))
                return newValue;
            else
                return 0;
        }

        public static string DefaultIdPadding(this long id)
        {
            return (id < 0 ? "-" : "") + Math.Abs(id).ToString().PadLeft(8, '0');
        }

        public static string DefaultIdPadding(this int id)
        {
            return ((long)id).DefaultIdPadding();
        }

        public static bool IsNumeric(this string text)
        {
            if (text == null) return false;
            return double.TryParse(text, out _);
        }

        public static bool IsOnlyNumbers(this string text)
        {
            if (text == null) return false;
            return text.All(char.IsNumber);
        }

        public static bool IsPhoneNumber(this string number)
        {
            return IsOnlyNumbers(number.Replace("+45", "").Replace(" ", "").Replace("+", ""));
        }

        public static string EnsureNonEmptyJson(this string json)
        {
            return string.IsNullOrWhiteSpace(json) ? "{}" : json;
        }
    }
}
