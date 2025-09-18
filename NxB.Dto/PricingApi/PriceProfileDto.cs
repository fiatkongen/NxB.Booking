using System;
using System.Collections.Generic;
using System.Text;

namespace NxB.Dto.PricingApi
{
    public class CreatePriceProfileDto
    {
        public Guid ResourceId { get; set; }
        public string Name { get; set; }
        public decimal? FixedPrice { get; set; }
    }

    public class PriceProfileDto : CreatePriceProfileDto
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

    public class ModifyPriceProfileStatisticsDto
    {
        public Guid Id { get; set; }
        public int DkStatCount { get; set; }
        public bool IsSeason { get; set; }
        public int StatCount { get; set; }
    }

    public class CopyPriceProfileDto
    {
        public Guid SourcePriceProfileId { get; set; }
        public Guid DestinationResourceId { get; set; }
    }
}
