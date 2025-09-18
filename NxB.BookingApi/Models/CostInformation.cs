using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NxB.BookingApi.Models
{
    public class CostInformation
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int Number { get; set; }
        public string Text { get; set; }
        public bool IsCacheHit { get; set; }

        private Decimal? cost;
        /// <summary>
        /// If cost is null, then the Cost returned is the sum of all the Costinformation children
        /// </summary>
        public Decimal? Cost
        {
            get { return ChildCostInformations.Count != 0 ? ChildCostInformations.Sum(c => c.Cost.Value) : cost; }
            set { cost = value; }
        }

        public string CostFormatted() => Cost?.ToDanishDecimalString(2) ?? "--";
        public string StartDateFormatted() => StartDate.ToDanishDate();
        public string EndDateFormatted() => EndDate.ToDanishDate();

        public List<CostInformation> ChildCostInformations = new List<CostInformation>();

        public CostInformation(DateTime startDate, DateTime endDate, int number, string text, Decimal? cost)
        {
            StartDate = startDate;
            EndDate = endDate;
            Number = number;
            Text = text;
            Cost = cost;
        }

        private string BuildTextInfoString()
        {
            return StartDate.ToShortDateString() + " - " + EndDate.ToShortDateString() + " = " + ((double)Cost).ToString("C");
        }

        /// <summary>
        /// Returns the "leaves" of the path used for the calculation,
        /// </summary>
        /// <param name="prefix"></param>
        /// <returns></returns>
        public List<CostInformation> GetCalculationPathLeaves()
        {
            //is this a leaf?
            if (ChildCostInformations.Count == 0)
                return new List<CostInformation> { this };
            else
                return this.ChildCostInformations.SelectMany(c => c.GetCalculationPathLeaves()).OrderBy(c => c.StartDate).ToList();
        }

        /// <summary>
        /// Returns the "leaves" of the path used for the calculation,
        /// </summary>
        /// <param name="prefix"></param>
        /// <returns></returns>
        public List<string> CalculationPathLeavesString
        {
            get
            {
                //is this a leaf?
                if (ChildCostInformations.Count == 0)
                    return new List<string> { this.BuildTextInfoString() };
                else
                    return this.ChildCostInformations.SelectMany(c => c.GetCalculationPathLeaves()).OrderBy(c => c.StartDate).Select(i => i.BuildTextInfoString()).ToList();
            }
        }

        public long GetLeafPathCount()
        {
            //is this a leaf?
            if (ChildCostInformations.Count == 0)
                return 1;
            else
                return this.ChildCostInformations.Sum(c => c.GetLeafPathCount());
        }

    }
}