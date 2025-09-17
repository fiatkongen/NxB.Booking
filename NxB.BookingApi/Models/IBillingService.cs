using NxB.Dto.AccountingApi;
using NxB.Dto.TenantApi;

namespace NxB.BookingApi.Models
{
    public interface IBillingService
    {
        Task<BillableItemDto> CreateBillableItem(CreateBillableItemDto createBillableItemDto, Guid tenantId);
        Task<BillableItemDto> TryCreateBillableItem(CreateBillableItemDto createBillableItemDto, Guid tenantId);
    }
}