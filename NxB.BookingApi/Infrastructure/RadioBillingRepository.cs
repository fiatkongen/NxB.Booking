using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Munk.AspNetCore;
using NxB.Domain.Common.Interfaces;
using NxB.BookingApi.Models;

namespace NxB.BookingApi.Infrastructure
{
    public class RadioBillingRepository: TenantFilteredRepository<RadioBilling, AppDbContext>, IRadioBillingRepository
    {
        public RadioBillingRepository(IClaimsProvider claimsProvider, AppDbContext appDbContext) : base(claimsProvider, appDbContext)
        {
        }

        public async Task Add(RadioBilling radioBilling)
        {
            await AppDbContext.AddAsync(radioBilling);
        }

        public async Task Delete(int radioAddress)
        {
            var radioBilling = await this.FindSingle(radioAddress);
            AppDbContext.Remove(radioBilling);
        }

        public async Task Update(RadioBilling radioBilling)
        {
            AppDbContext.Update(radioBilling);
            await Task.CompletedTask;
        }

        public Task<List<RadioBilling>> FindAll()
        {
            return TenantFilteredEntitiesQuery.OrderBy(x => x.RadioAddress).ToListAsync();
        }

        public async Task<RadioBilling> FindSingle(int radioAddress)
        {
            return await TenantFilteredEntitiesQuery.SingleAsync(x => x.RadioAddress == radioAddress);
        }

        public async Task<RadioBilling> FindSingleOrDefault(int radioAddress)
        {
            return await TenantFilteredEntitiesQuery.SingleOrDefaultAsync(x => x.RadioAddress == radioAddress);
        }

        // Interface implementations with Guid parameters
        public async Task<RadioBilling> FindSingle(Guid id)
        {
            return await TenantFilteredEntitiesQuery.SingleAsync(x => x.Id == id);
        }

        public async Task Delete(Guid id)
        {
            var radioBilling = await this.FindSingle(id);
            AppDbContext.Remove(radioBilling);
        }
    }
}