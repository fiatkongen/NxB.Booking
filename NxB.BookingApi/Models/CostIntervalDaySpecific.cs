using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace NxB.BookingApi.Models
{
    public class CostIntervalDaySpecific : CostIntervalDay
    {

        public override int MaxNumber
        {
            get { return 1; }
        }

        public override int MinNumber
        {
            get { return 1; }
        }

        private List<CostItemSpecificDay> listSpecifics = null;

        [JsonIgnore]
        public List<CostItemSpecificDay> ListSpecifics
        {
            get
            {
                if (listSpecifics == null)
                    InitializeSpecifics();
                return listSpecifics;
            }
        }

        private void InitializeSpecifics()
        {
            listSpecifics = new List<CostItemSpecificDay>();

            for (int i = 0; i < 7; i++)
                listSpecifics.Add(new CostItemSpecificDay((DayOfWeek)i, this));

            if (!string.IsNullOrEmpty(Specifics))
            {
                var savedListSpecifics = Specifics.Split(',').ToList().Select(l => (DayOfWeek)int.Parse(l)).ToList();

                foreach (var savedListSpecific in savedListSpecifics)
                {
                    listSpecifics.First(l => l.Day == savedListSpecific).IsChecked = true;
                }
            }
        }

        public CostIntervalDaySpecific(Guid id, DateTime startDate, DateTime endDate, int number, decimal cost, string costType = "CostIntervalDaySpecific") : base(id, startDate, endDate, number, cost, costType)
        {
        }

        public override bool CheckIfValid(DateTime startDate, DateTime endDate, CostCalculationContext costCalculationContext)
        {
            return ListSpecifics.FirstOrDefault(l => l.IsChecked && l.Day == startDate.DayOfWeek) != null;
        }
    }
}