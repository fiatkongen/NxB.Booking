using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace NxB.BookingApi.Models
{
    public interface IAccountChartItemRepository
    {
        Task Add(AccountChartItem accountChartItem);
        void Update(AccountChartItem accountChartItem);
        Task<List<AccountChartItem>> FindAccountChartItems();
        Task<AccountChartItem> FindSingleAccountChartItem(Guid id);
        Task<AccountChartItem> MarkAccountChartItemAsDeleted(Guid id);
        Task<AccountChartItem> AddPriceProfileToAccountChartItem(Guid accountChartItemId, Guid priceProfileId);
        Task<AccountChartItem> RemovePriceProfileFromAccountChartItem(Guid accountChartItemId, Guid priceProfileId);
        Task<AccountChartItem> MarkAccountChartItemAsUndeleted(Guid id);
    }
}
