using System;
using System.Collections.Generic;
using System.Text;
using NxB.Domain.Common.Interfaces;
using NxB.Dto.AccountingApi;
using ServiceStack;

namespace NxB.BookingApi.Models
{
    public class AccountChartItem : ITenantEntity
    {
        public Guid Id { get; set; }
        public Guid TenantId { get; set; }
        public int Number { get; set; }
        public string Name { get; set; }
        public AccountChartType AccountChartType { get; set; }
        public AccountChartItemType AccountChartItemType { get; set; }
        public bool IsDeleted { get; set; }
        public List<AccountChartItemPriceProfile> AccountChartItemPriceProfiles { get; set; } = new();

        public AccountChartItem(Guid id, Guid tenantId)
        {
            Id = id;
            TenantId = tenantId;
        }

        public void MarkAsDeleted()
        {
            this.IsDeleted = true;
        }

        public void MarkAsUnDeleted()
        {
            this.IsDeleted = false;
        }
    }
}
