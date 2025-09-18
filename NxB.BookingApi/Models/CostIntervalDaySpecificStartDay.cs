using System;
using System.Linq;

namespace NxB.BookingApi.Models
{
    public class CostIntervalDaySpecificStartDay : CostIntervalDaySpecific
    {
        public override string GetNumbersDescription(int number)
        {
            return number <= 1 ? "Dag" : "Dage" + " (ankomst)";
        }

        public CostIntervalDaySpecificStartDay(Guid id, DateTime startDate, DateTime endDate, int number, decimal cost, string costType = "CostIntervalDaySpecificStartDay") : base(id, startDate, endDate, number, cost, costType)
        {
        }

        public override bool CheckIfValid(DateTime startDate, DateTime endDate, CostCalculationContext costCalculationContext)
        {
            return ListSpecifics.FirstOrDefault(l => l.IsChecked && l.Day == startDate.DayOfWeek && (costCalculationContext == null || costCalculationContext.OriginalStartDate == startDate)) != null;
        }
    }
}