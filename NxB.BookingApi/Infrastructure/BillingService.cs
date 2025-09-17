using NxB.Dto.AccountingApi;
using NxB.BookingApi.Models;

namespace NxB.BookingApi.Infrastructure
{
    public class BillingService : IBillingService
    {
        public Task<BillableItemDto> CreateBillableItem(CreateBillableItemDto createBillableItemDto, Guid tenantId)
        {
            throw new NotImplementedException();
        }

        public Task<BillableItemDto> TryCreateBillableItem(CreateBillableItemDto createBillableItemDto, Guid tenantId)
        {
            throw new NotImplementedException();
        }
    }
}