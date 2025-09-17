using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using NxB.Domain.Common.Enums;
using NxB.Domain.Common.Interfaces;

namespace NxB.BookingApi.Models
{
    public interface IBillableItemsRepository : ICloneWithCustomClaimsProvider<IBillableItemsRepository>
    {
        Task Add(BillableItem billableItem);
        void Delete(BillableItem billableItem);
        Task<List<BillableItem>> FindAll();
        Task<List<BillableItem>> FindFromText(BillableItemType billableItemType, string text);
        Task<List<BillableItem>> FindAllUnpaid();
        Task<List<BillableItem>> FindAllFromType_Global(BillableItemType billableItemType);
        Task<BillableItem> FindSingle(Guid id);
        Task<BillableItem> FindSingleOrDefault(Guid id);
        Task ActivateItem(Guid billedItemRef);
        Task<BillableItem> FindSingleFromBilledItemRefRefId(Guid billedItemRef);
    }
}