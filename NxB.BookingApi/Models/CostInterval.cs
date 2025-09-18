using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using Munk.AspNetCore;
using NxB.Domain.Common.Interfaces;

namespace NxB.BookingApi.Models
{
    public class CostInterval : ICreateAudit, ITenantEntity
    {
        public Guid Id { get; set; }
        public Guid TenantId { get; set; }
        public long? LegacyId { get; set; }
        public Guid PriceProfileId { get; set; }
        public DateTime CreateDate { get; set; }
        public Guid CreateAuthorId { get; set; }
        public DateTime LastModifiedDate { get; set; }
        public Guid LastModifiedAuthorId { get; set; }
        public string CostType { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int Number { get; set; }
        public decimal Cost { get; set; }
        public string Specifics { get; set; }
        public int? Min { get; set; }
        public int? Max { get; set; }
        public decimal? MinCost { get; set; }
        public decimal? MaxCost { get; set; }
        public bool IsDeleted { get; set; }
        public bool IsImported { get; set; }

        public DayOfWeek? SpecificArrivalDay { get; set; }
        public int? SpecificArrivalDate { get; set; }


        public string NumbersDescription => GetNumbersDescription(Number);

        //public string DaysDescription => GetDaysDescription(null);

        public virtual int MaxNumber => 9999;

        public virtual int MinNumber => 0;

        public bool HasBeenApplied = false;

        public CostInterval(Guid id, string costType)
        {
            Id = id;
            CostType = costType;
        }


        /// <summary>
        /// Override this method to apply extra rules/filtering to the intervals. Fx. to filter just Mondays
        /// </summary>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <param name="hasArrivalDayBeenUsed"></param>
        /// <returns></returns>
        public virtual bool CheckIfValid(DateTime startDate, DateTime endDate, CostCalculationContext costCalculationContext)
        {
            return true;
        }

        public virtual string GetNumbersDescription(int number)
        {
            throw new NotImplementedException();
        }

        public virtual List<DateInterval> BuildMaximumDateIntervals()
        {
            throw new NotImplementedException();
        }

        public virtual List<CostInterval> BuildLongestChildCostIntervals()
        {
            return new List<CostInterval> { this };
        }

        //public virtual string GetDaysDescription(List<CostItemSpecific> specifics)
        //{
        //    return "";
        //}


        public bool IsWithin(DateTime startDate, DateTime endDate)
        {
            return (this.StartDate <= startDate && startDate < this.EndDate) || (this.StartDate < endDate && endDate <= this.EndDate);
        }

        /// <summary>
        /// Returns the startDate + the duration of the CostInterval (necessary because of months! )
        /// </summary>
        /// <param name="startDate"></param>
        /// <returns></returns>
        public virtual DateTime AddTimeSpan(DateTime startDate, DateTime endDate)
        {
            throw new NotImplementedException();
        }

        public virtual CostInformation CalculateCost(DateTime startDate, DateTime endDate, CostCalculator costCalculator, ConcurrentDictionary<string, CostInformation> optimizerCostTree, CostCalculationContext costCalculationContext)
        {
            costCalculator.TotalCalculations++;

            var isWithinThisCostInterval = startDate >= this.StartDate && endDate <= this.EndDate;

            var daysDiff = startDate.GetDaysDiff(endDate);
            var daysIntervalText = daysDiff + " " + (daysDiff > 1 ? "Dage"  : "Dag");

            if (isWithinThisCostInterval)
            {
                var tmpCostInformation = new CostInformation(startDate, endDate, 1, "Interval " + daysIntervalText, null);

                var nextEndDate = AddTimeSpan(startDate, endDate);
                if (nextEndDate >= endDate)
                {
                    return CreateBasicCostInformation(startDate, endDate);
                }
                else
                {
                    tmpCostInformation.ChildCostInformations.Add(costCalculator.CalculateCost(startDate, nextEndDate, optimizerCostTree, costCalculationContext));
                    tmpCostInformation.ChildCostInformations.Add(costCalculator.CalculateCost(nextEndDate, endDate, optimizerCostTree, costCalculationContext));

                    return tmpCostInformation;
                }
            }
            else
            {
                var tmpCostInformation = new CostInformation(startDate, endDate, 1, "Interval " + daysIntervalText, null);

                if (startDate < this.StartDate)
                {
                    tmpCostInformation.ChildCostInformations.Add(costCalculator.CalculateCost(startDate, this.StartDate, optimizerCostTree, costCalculationContext));
                    tmpCostInformation.ChildCostInformations.Add(costCalculator.CalculateCost(this.StartDate, endDate, optimizerCostTree, costCalculationContext));
                }

                if (endDate > this.EndDate)
                {
                    tmpCostInformation.ChildCostInformations.Add(costCalculator.CalculateCost(startDate, this.EndDate, optimizerCostTree, costCalculationContext));
                    tmpCostInformation.ChildCostInformations.Add(costCalculator.CalculateCost(this.EndDate, endDate, optimizerCostTree, costCalculationContext));
                }

                return tmpCostInformation;
            }
        }

        public virtual CostInformation CreateBasicCostInformation(DateTime startDate, DateTime endDate)
        {
            this.HasBeenApplied = true;
            return new CostInformation(startDate, endDate.Lowest(this.EndDate), Number, Number + " " + GetNumbersDescription(Number), Cost);
        }

        public CostInterval Clone(Guid priceProfileId)
        {
            var clone = this.CloneJson();
            clone.Id = Guid.NewGuid();
            clone.PriceProfileId = priceProfileId;
            return clone;
        }
    }
}