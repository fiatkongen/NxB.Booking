using System;
using System.Collections.Generic;
using System.Text;

namespace NxB.BookingApi.Models
{
    public class AccountChartItemPriceProfile
    {
        public Guid AccountChartItemId { get; set; }
        public Guid PriceProfileId { get; set; }
        public Guid TenantId { get; set; }

        public AccountChartItemPriceProfile(Guid accountChartItemId, Guid priceProfileId, Guid tenantId)
        {
            AccountChartItemId = accountChartItemId;
            PriceProfileId = priceProfileId;
            TenantId = tenantId;
        }
    }
}
