using System;
using System.Threading.Tasks;
using NxB.Dto.TenantApi;

namespace NxB.Dto.Clients
{
    public interface IBillableClient : IAuthorizeClient
    {
        Task<BillableItemDto> FindSingle(Guid id);
        Task<BillableItemDto> FindSingleFromBillableItemRef(Guid billedItemRef);
        Task ActivateItem(Guid id);
        Task TryDeleteItem(Guid billedItemRef);
        Task DeleteItem(Guid billedItemRef);
    }
}