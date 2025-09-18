using System;
using System.Globalization;

namespace Munk.Utils.Object
{
    public abstract class BasicFormatterBase
    {
        public abstract string CurrencyFormat { get; }
        public abstract string CurrencyName { get; }
        public abstract string CultureInfoString { get; }
        public abstract string DateFormat { get; }

        public string FormatCurrency(decimal amount)
        {
            decimal formattedAmount = (decimal)amount;
            return formattedAmount.ToString(CurrencyFormat, new CultureInfo(CultureInfoString));
        }

        public string FormatDate(DateTime date)
        {               
            return date.ToString(DateFormat);
        }

        public string FormatDateTime(DateTime date)
        {
            return date.ToString(DateFormat + " H:mm");
        }

        public string FormatDecimal(decimal value, int digits = 2)
        {
            return value.ToString("N" + digits, new CultureInfo(CultureInfoString));
        }
    }

    public class DanishBasicFormatter : BasicFormatterBase
    {
        public override string CurrencyFormat => "###,###,###,##0.00";
        public override string CurrencyName => "DKK";
        public override string CultureInfoString => "da-DK";
        public override string DateFormat => "d.M.yyyy";
    }
}
