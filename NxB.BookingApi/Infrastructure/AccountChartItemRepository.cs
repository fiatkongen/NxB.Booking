using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Munk.AspNetCore;
using NxB.BookingApi.Models;
using NxB.Domain.Common.Interfaces;

namespace NxB.BookingApi.Infrastructure
{
    public class AccountChartItemRepository : TenantFilteredRepository<AccountChartItem, AppDbContext>, IAccountChartItemRepository
    {
        public AccountChartItemRepository(IClaimsProvider claimsProvider, AppDbContext appDbContext) : base(claimsProvider, appDbContext)
        {
        }

        public async Task Add(AccountChartItem accountChartItem)
        {
            await AppDbContext.AddAsync(accountChartItem);
        }

        public void Update(AccountChartItem accountChartItem)
        {
            AppDbContext.Update(accountChartItem);
        }

        public Task<List<AccountChartItem>> FindAccountChartItems()
        {
            return TenantFilteredEntitiesQuery.Include(x => x.AccountChartItemPriceProfiles).Where(x => !x.IsDeleted).OrderBy(x => x.Number).AsSingleQuery().ToListAsync();
        }

        public Task<AccountChartItem> FindSingleAccountChartItem(Guid id)
        {
            return AppDbContext.AccountChartItems.Include(x => x.AccountChartItemPriceProfiles).SingleAsync(x => x.Id == id);
        }

        public async Task<AccountChartItem> MarkAccountChartItemAsDeleted(Guid id)
        {
            var item = await AppDbContext.AccountChartItems.SingleAsync(x => x.Id == id);
            item.MarkAsDeleted();
            return item;
        }

        public async Task<AccountChartItem> AddPriceProfileToAccountChartItem(Guid accountChartItemId, Guid priceProfileId)
        {
            var item = await AppDbContext.AccountChartItems.SingleAsync(x => x.Id == accountChartItemId);
            if (item != null)
            {
                var accountChartItemPriceProfile = new AccountChartItemPriceProfile(accountChartItemId, priceProfileId, this.TenantId);
                item.AccountChartItemPriceProfiles.Add(accountChartItemPriceProfile);
            }

            return item;
        }


        public async Task<AccountChartItem> RemovePriceProfileFromAccountChartItem(Guid accountChartItemId, Guid priceProfileId)
        {
            var item = await AppDbContext.AccountChartItems.Include(x => x.AccountChartItemPriceProfiles).SingleAsync(x => x.Id == accountChartItemId);
            if (item != null)
            {
                var accountChartItemPriceProfile =
                    item.AccountChartItemPriceProfiles.Single(x => x.PriceProfileId == priceProfileId);
                item.AccountChartItemPriceProfiles.Remove(accountChartItemPriceProfile);
            }

            return item;
        }

        public Task<AccountChartItem> MarkAccountChartItemAsUndeleted(Guid id)
        {
            throw new NotImplementedException();
        }

    }
}
