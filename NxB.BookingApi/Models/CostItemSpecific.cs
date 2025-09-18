using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NxB.BookingApi.Models
{
    public abstract class CostItemSpecific
    {
        public abstract string Name { get; }
        public CostInterval CostInterval { get; set; }

        public bool IsChecked { get; set; }

        protected CostItemSpecific(CostInterval costInterval)
        {
            CostInterval = costInterval;
        }

    }
}