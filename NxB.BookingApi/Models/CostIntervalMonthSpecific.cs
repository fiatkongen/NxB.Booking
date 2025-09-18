using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace NxB.BookingApi.Models
{
    public class CostIntervalMonthSpecific : CostIntervalMonth
    {
        public override int MaxNumber
        {
            get { return 1; }
        }

        public override int MinNumber
        {
            get { return 1; }
        }

        private List<CostItemSpecificMonth> _listSpecifics = null;

        [JsonIgnore]
        public List<CostItemSpecificMonth> ListSpecifics
        {
            get
            {
                if (_listSpecifics == null)
                    InitializeSpecifics();
                return _listSpecifics;
            }
        }

        public CostIntervalMonthSpecific(Guid id, DateTime startDate, DateTime endDate, decimal cost)
            : base(id, startDate, endDate, 1, cost, "CostIntervalMonthSpecific")
        {
        }

        public override DateTime AddTimeSpan(DateTime startDate, DateTime endDate)
        {
            return startDate.AddDays(DateTime.DaysInMonth(startDate.Year, startDate.Month) - (startDate.Day - 1)).Date;
        }


        public override bool CheckIfValid(DateTime startDate, DateTime endDate, CostCalculationContext costCalculationContext)
        {
            return ListSpecifics.FirstOrDefault(l => l.IsChecked && startDate.Month == l.Month) != null;
        }

        private void InitializeSpecifics()
        {
            _listSpecifics = new List<CostItemSpecificMonth>();

            for (int i = 1; i < 13; i++)
                _listSpecifics.Add(new CostItemSpecificMonth(i, this));

            if (!string.IsNullOrEmpty(Specifics))
            {
                var savedListSpecifics = Specifics.Split(',').ToList().Select(int.Parse).ToList();

                foreach (var savedListSpecific in savedListSpecifics)
                {
                    _listSpecifics.First(l => l.Month == savedListSpecific).IsChecked = true;
                }
            }
        }
    }

    public class CostItemSpecificMonth : CostItemSpecific
    {
        public override string Name
        {
            get { return Month.ToString(); }
        }

        public int Month { get; set; }


        public CostItemSpecificMonth(int month, CostInterval costInterval)
            : base(costInterval)
        {
            Month = month;
            CostInterval = costInterval;
        }

        public override string ToString()
        {
            return Month.ToString();
        }
    }
}