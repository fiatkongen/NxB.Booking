using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NxB.BookingApi.Models;
using NxB.Domain.Common.Interfaces;

namespace NxB.BookingApi.Infrastructure
{
    [Serializable]
    public class FriendlyAccountingIdProvider : IFriendlyAccountingIdProvider
    {
        private readonly AppDbContext _appDbContext;
        private readonly IClaimsProvider _claimsProvider;
        private readonly IMemCacheActor _memCacheActor;

        public FriendlyAccountingIdProvider(AppDbContext appDbContext, IClaimsProvider claimsProvider, IMemCacheActor memCacheActor)
        {
            _appDbContext = appDbContext;
            _claimsProvider = claimsProvider;
            _memCacheActor = memCacheActor;
        }

        public long GenerateNextFriendlyCustomerId()
        {
            var customer = _appDbContext.Customers.Where(x => x.TenantId == _claimsProvider.GetTenantId()).OrderByDescending(x => x.FriendlyId).FirstOrDefault();
            long nextId = customer?.FriendlyId + 1 ?? 1;
            if (customer != null && customer.IsImported && nextId < 10000000) nextId = 10000000;
            return nextId;
        }

        public long GenerateNextFriendlyInvoiceId()
        {
            var invoice1 = _appDbContext.Vouchers.Where(x => x.TenantId == _claimsProvider.GetTenantId()).OfType<InvoiceBase>().OrderByDescending(x => x.FriendlyId).AsNoTracking().FirstOrDefault();
            var invoice2 = _appDbContext.Vouchers.Local.Where(x => x.TenantId == _claimsProvider.GetTenantId()).OfType<InvoiceBase>().MaxBy(x => x.FriendlyId);

            long nextId1 = invoice1?.FriendlyId + 1 ?? 1;
            long nextId2 = invoice2?.FriendlyId + 1 ?? 1;

            return nextId1.Highest(nextId2).Highest((long)1);
        }

        public long GenerateNextFriendlyDueDepositId()
        {
            var invoice1 = _appDbContext.Vouchers.Where(x => x.TenantId == _claimsProvider.GetTenantId()).OfType<DueDeposit>().OrderByDescending(x => x.FriendlyId).AsNoTracking().FirstOrDefault();
            var invoice2 = _appDbContext.Vouchers.Local.Where(x => x.TenantId == _claimsProvider.GetTenantId()).OfType<DueDeposit>().MaxBy(x => x.FriendlyId);

            long nextId1 = invoice1?.FriendlyId + 1 ?? 1;
            long nextId2 = invoice2?.FriendlyId + 1 ?? 1;

            return nextId1.Highest(nextId2).Highest((long)1);
        }

        public async Task<long> GenerateNextFriendlyDueDepositIdWithGapsAllowed()
        {
            string counterKey = this._claimsProvider.GetTenantId() + "_duedepositcounter";
            var count = await this._memCacheActor.GetCachedLong(counterKey, null);
            if (count == null)
            {
                count = this.GenerateNextFriendlyDueDepositId();
                await this._memCacheActor.SetCachedLong(counterKey, count);
            }
            else
            {
                count = count.Value + 1;
                await this._memCacheActor.SetCachedLong(counterKey, count);
            }
            return count.Value;
        }

        public long GenerateNextFriendlyPaymentId()
        {
            var payment1 = _appDbContext.Vouchers.Where(x => x.TenantId == _claimsProvider.GetTenantId()).OfType<Payment>().OrderByDescending(x => x.FriendlyId).AsNoTracking().FirstOrDefault();
            var payment2= _appDbContext.Vouchers.Local.Where(x => x.TenantId == _claimsProvider.GetTenantId()).OfType<Payment>().MaxBy(x => x.FriendlyId);

            long nextId1 = payment1?.FriendlyId + 1 ?? 1;
            long nextId2 = payment2?.FriendlyId + 1 ?? 1;

            return nextId1.Highest(nextId2);
        }
    }
}
