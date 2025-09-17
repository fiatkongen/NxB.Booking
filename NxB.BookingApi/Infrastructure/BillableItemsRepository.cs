using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Munk.AspNetCore;
using NxB.Domain.Common.Enums;
using NxB.Domain.Common.Interfaces;
using NxB.BookingApi.Models;

namespace NxB.BookingApi.Infrastructure
{
    public class BillableItemsRepository : TenantFilteredRepository<BillableItem, AppDbContext>, IBillableItemsRepository
    {
        public BillableItemsRepository(IClaimsProvider claimsProvider, AppDbContext appDbContext) : base(claimsProvider, appDbContext)
        {
        }

        public IBillableItemsRepository CloneWithCustomClaimsProvider(IClaimsProvider overrideClaimsProvider)
        {
            return new BillableItemsRepository(overrideClaimsProvider, AppDbContext);
        }

        public async Task Add(BillableItem billableItem)
        {
            await AppDbContext.AddAsync(billableItem);
        }

        public void Delete(BillableItem billableItem)
        {
            AppDbContext.Remove(billableItem);
        }

        public Task<List<BillableItem>> FindAll()
        {
            return TenantFilteredEntitiesQuery.ToListAsync();
        }

        public Task<List<BillableItem>> FindFromText(BillableItemType billableItemType, string text)
        {
            return TenantFilteredEntitiesQuery.Where(x => x.Type == billableItemType && x.Text == text).ToListAsync();
        }

        public Task<List<BillableItem>> FindAllUnpaid()
        {
            return TenantFilteredEntitiesQuery.Where(x => !x.Paid).ToListAsync();
        }

        public Task<List<BillableItem>> FindAllFromType_Global(BillableItemType billableItemType)
        {
            return AppDbContext.BillableItems.Where(x => x.Type == billableItemType).OrderBy(x => x.CreateDate).ToListAsync();
        }

        public Task<BillableItem> FindSingle(Guid id)
        {
            return TenantFilteredEntitiesQuery.SingleAsync(x => x.Id == id);
        }

        public Task<BillableItem> FindSingleOrDefault(Guid id)
        {
            return TenantFilteredEntitiesQuery.SingleOrDefaultAsync(x => x.Id == id);
        }

        public Task<BillableItem> FindSingleFromBilledItemRefRefId(Guid billedItemRef)
        {
            return AppDbContext.BillableItems.SingleOrDefaultAsync(x => x.BilledItemRef == billedItemRef);
        }

        public async Task ActivateItem(Guid billedItemRef)
        {
            var item = await FindSingle(billedItemRef);
            item.Activate();
        }
    }
}