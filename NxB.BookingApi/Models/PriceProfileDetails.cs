using System;
using System.Collections.Generic;
using System.Text;

namespace NxB.BookingApi.Models
{
    public class CreatePriceProfileDetails
    {
        public Guid ResourceId { get; set; }
        public string Name { get; set; }
        public decimal? FixedPrice { get; set; }
    }

    public class PriceProfileDetails : CreatePriceProfileDetails
    {
        public Guid Id { get; set; }
        public Guid TenantId { get; set; }
        public long LegacyTypeId { get; set; }
        public string Description { get; set; }
        public bool IsDeleted { get; set; }
        public int DkStatCount { get; set; }
        public int StatCount { get; set; }
        public bool IsSeason { get; set; }
        public long CostCalculationId { get; set; }
        public DateTime? FixedPriceLastModified { get; set; }
    }

    public class ModifyPriceProfileStatistics
    {
        public Guid Id { get; set; }
        public int DkStatCount { get; set; }
        public bool IsSeason { get; set; }
        public int StatCount { get; set; }
    }

    public class CopyPriceProfile
    {
        public Guid SourcePriceProfileId { get; set; }
        public Guid DestinationResourceId { get; set; }
    }
}