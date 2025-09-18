using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NxB.BookingApi.Models
{
    public class CreateCostIntervalDetails
    {
        public Guid PriceProfileId { get; set; }
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
        public bool IsImported { get; set; }
        public DayOfWeek? SpecificArrivalDay { get; set; }
        public int? SpecificArrivalDate { get; set; }
    }

    public class CostIntervalDetails : CreateCostIntervalDetails
    {
        public Guid Id { get; set; }
        public DateTime CreateDate { get; set; }
        public Guid CreateAuthorId { get; set; }
        public DateTime LastModifiedDate { get; set; }
        public Guid LastModifiedAuthorId { get; set; }
    }

    public class CreateOrModifyCostIntervalDetails : CreateCostIntervalDetails
    {
        public Guid? Id { get; set; }
        public string Action { get; set; }
    }
}