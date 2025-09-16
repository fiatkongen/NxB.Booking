using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Munk.Utils.Object
{
    public static class DecimalExtensions
    {
        public static decimal ReverseTaxAmount(this decimal taxAmount, decimal taxPercent)
        {
            if (taxPercent == 0 || taxAmount == 0) return 0;
            var result = taxAmount - Math.Round(taxAmount / (1 + taxPercent / 100), 2);
            return result;
        }
    }
}
