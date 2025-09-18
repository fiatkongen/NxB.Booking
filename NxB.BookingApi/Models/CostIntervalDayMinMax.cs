using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NxB.BookingApi.Models
{
    public class CostIntervalDayMinMax : CostIntervalDay
    {
        public int MinDays
        {
            get { return Max.Value; }
        }

        public int MaxDays
        {
            get { return Max.Value; }
        }


        public override bool CheckIfValid(DateTime startDate, DateTime endDate, CostCalculationContext costCalculationContext)
        {
            return endDate.Subtract(startDate).Days >= MinDays;
        }

        public override DateTime AddTimeSpan(DateTime startDate, DateTime endDate)
        {
            var minimumEndDate = startDate.AddDays(Number + MinDays).Date;

            if (minimumEndDate >= endDate)
                return minimumEndDate;
            else
            {
                return endDate;
            }
        }

        public override CostInformation CreateBasicCostInformation(DateTime startDate, DateTime endDate)
        {
            var tmpCostInformation = new CostInformation(startDate, endDate, 1, "Interval - DayMinMax", null);
            //how many times can the minimum days be repeated until endDate is reached
            DateTime startDateOfRepeat = startDate.AddDays(MinDays);
            DateTime endDateOfRepeat = endDate.Lowest(this.EndDate);

            var costInformation = new CostInformation(startDate, endDate.Lowest(startDateOfRepeat.Lowest(endDate)), MinDays, MinDays + " " + GetNumbersDescription(MinDays), MinCost);
            tmpCostInformation.ChildCostInformations.Add(costInformation);

            while (startDateOfRepeat < endDateOfRepeat)
            {
                DateTime newStartDateOfRepeat = startDateOfRepeat.AddDays(Number);
                tmpCostInformation.ChildCostInformations.Add(new CostInformation(startDateOfRepeat, endDate.Lowest(this.EndDate).Lowest(newStartDateOfRepeat), Number, Number + " " + GetNumbersDescription(Number), Cost));
                startDateOfRepeat = newStartDateOfRepeat;
            }

            return tmpCostInformation;
        }

        public CostIntervalDayMinMax(Guid id, DateTime startDate, DateTime endDate, int number, decimal cost) : base(id, startDate, endDate, number, cost, "CostIntervalDayMinMax")
        {
        }
    }
}