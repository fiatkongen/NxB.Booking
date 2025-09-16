using NxB.Dto.AccountingApi;

namespace NxB.BookingApi.Models
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