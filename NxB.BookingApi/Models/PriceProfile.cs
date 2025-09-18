using System;
using System.Collections.Generic;
using System.Text;
using NxB.Domain.Common.Interfaces;

namespace NxB.BookingApi.Models
{
    [Serializable]
    public class PriceProfile : ITenantEntity
    {
        public Guid Id { get; set; }
        public Guid TenantId { get; set; }
        public Guid? ResourceId { get; set; }
        public long LegacyId { get; set; }
        public long LegacyTypeId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public long CostCalculationId { get; set; }
        public bool IsDeleted { get; set; }
        public bool IsImported { get; set; }
        public int DkStatCount { get; set; }
        public int StatCount { get; set; }
        public bool IsSeason { get; set; }
        public decimal? FixedPrice { get; set; }
        public DateTime? FixedPriceLastModified { get; set; }

        //For now, only a constructor exists that demands a legacyId.
        public PriceProfile(Guid id, Guid tenantId, long legacyId, long legacyTypeId, long costCalculationId, string name)
        {
            Id = id;
            TenantId = tenantId;
            LegacyId = legacyId;
            LegacyTypeId = legacyTypeId;
            CostCalculationId = costCalculationId;
            Name = name;
        }
    }
}