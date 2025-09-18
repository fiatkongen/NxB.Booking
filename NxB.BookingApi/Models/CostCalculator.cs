using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
// TODO: Remove AutoFixture references
// using AutoFixture;
using Munk.AspNetCore;
// TODO: Remove old namespace references
// using NxB.Dto.PricingApi;
// TODO: Remove old namespace references
// using NxB.PricingApi.Exceptions;
using NxB.BookingApi.Exceptions;

namespace NxB.BookingApi.Models
{
    public class CostCalculator
    {
        public int TotalCalculations = 0;
        //public static List<CostInformation> BestCosts = new List<CostInformation>();

        public List<CostInterval> CostIntervals = new();
        public DateTime? ErrorStart;
        public DateTime? ErrorEnd;

        public CostCalculator(List<CostInterval> costIntervals)
        {
            var intervals = costIntervals.SelectMany(x => x.BuildLongestChildCostIntervals()).ToList();
            this.CostIntervals = intervals;
        }

        public CostInformation CalculateCost(DateTime startDate, DateTime endDate, bool returnNullIfNoCostInformation, ConcurrentDictionary<string, CostInformation> optimizerCostTree, CostCalculationContext costCalculationContext)
        {
            var costInformation = this.CalculateCost(startDate, endDate, optimizerCostTree, costCalculationContext);
            if (costInformation == null && !returnNullIfNoCostInformation)
            {
                if (!Debugger.IsAttached)
                {
                    throw new CostCalculationException((ErrorStart ?? startDate).Highest(startDate), (ErrorEnd ?? endDate).Lowest(endDate));
                }
                else
                {
                    return null;
                }
            }
            return costInformation;
        }

        public CostInformation CalculateCost(DateTime startDate, DateTime endDate, ConcurrentDictionary<string, CostInformation> optimizerCostTree, CostCalculationContext costCalculationContext)
        {
            if (optimizerCostTree == null)
                optimizerCostTree = new ConcurrentDictionary<string, CostInformation>(32, 10000);

            var key = startDate.ToFileTime() + "_" + endDate.ToFileTime();

            if (optimizerCostTree.TryGetValue(key, out var findAlreadyCalculatedCost))
            {
                return findAlreadyCalculatedCost;
            }

            var intervals = CostIntervals.Where(i =>
                i.Number > 0 && i.IsWithin(startDate, endDate)).Where(x => x.CheckIfValid(startDate, endDate, costCalculationContext)).ToList();

            if (intervals.None())
            {
                return null;
            }

            CostInformation bestCost = null;

            foreach (var interval in intervals)
            {

                var tmpCost = interval.CalculateCost(startDate, endDate, this, optimizerCostTree, costCalculationContext);
                if (tmpCost == null || tmpCost.ChildCostInformations.Any(x => x == null))
                {
                    if (ErrorStart == null)
                    {
                        ErrorStart = startDate;
                    }
                    else
                    {
                        ErrorStart = startDate.Highest(ErrorStart);
                    }
                    if (ErrorEnd == null)
                    {
                        ErrorEnd = endDate;
                    }
                    else
                    {
                        ErrorEnd = endDate.Lowest(ErrorEnd);
                    }
                    continue;
                }

                if (bestCost == null || tmpCost.Cost < bestCost.Cost)
                    bestCost = tmpCost;
                else if (tmpCost.Cost == bestCost.Cost)
                {
                    //If both calculations costs the same, select the "simplest" one
                    if (tmpCost.GetLeafPathCount() < bestCost.GetLeafPathCount())
                        bestCost = tmpCost;
                }
            }

            if (bestCost == null)
            {
                return null;
            }

            optimizerCostTree.TryAdd(key, bestCost);

            return bestCost;
        }
    }
}