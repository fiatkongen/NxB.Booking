using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Munk.AspNetCore;
using NxB.Allocating.Shared.Model;
using NxB.Domain.Common.Dto;
using NxB.Domain.Common.Interfaces;

namespace NxB.Allocating.Shared.Infrastructure
{
    public class RentalUnitRepository<TAppDbContext> : TenantFilteredRepository<RentalUnit, TAppDbContext>, IRentalUnitRepository where TAppDbContext : DbContext
    {
        public RentalUnitRepository(IClaimsProvider claimsProvider, TAppDbContext appDbContext) : base(claimsProvider, appDbContext)
        {
        }

        public RentalUnitRepository<TAppDbContext> CloneWithCustomClaimsProvider(IClaimsProvider customClaimsProvider)
        {
            return new RentalUnitRepository<TAppDbContext>(customClaimsProvider, AppDbContext);
        }

        public void Add(RentalUnit rentalUnit)
        {
            AppDbContext.Add(rentalUnit);
        }

        public void Add(IEnumerable<RentalUnit> rentalUnits)
        {
            foreach (var rentalUnit in rentalUnits)
            {
                this.Add(rentalUnit);
            }
        }

        public void Delete(Guid id)
        {
            var rentalUnit = FindSingle(id);
            this.AppDbContext.Set<RentalUnit>().Remove(rentalUnit);
        }

        public void Update(RentalUnit rentalUnit)
        {
            this.AppDbContext.Update(rentalUnit);
        }

        public void MarkAsDeleted(Guid id)
        {
            var rentalUnit = FindSingle(id);
            rentalUnit.IsDeleted = true;
        }

        public void MarkAsUnDeleted(Guid id)
        {
            var rentalUnit = FindSingle(id);
            rentalUnit.IsDeleted = false;
        }

        public string TryGetRentalUnitName(Guid id)
        {
            return this.FindSingleOrDefault(id)?.Name ?? "N/A";
        }

        public RentalUnit FindSingleOrDefault(Guid id)
        {
            if (id == Guid.Empty) throw new ArgumentException(nameof(id));
            var rentalUnit = this.Set().SingleOrDefault(x => x.Id == id);
            return rentalUnit;
        }

        public RentalUnit FindSingle(Guid id)
        {
            if (id == Guid.Empty) throw new ArgumentException(nameof(id));
            var rentalUnit = this.Set().Single(x => x.Id == id);
            return rentalUnit;
        }

        public async Task<List<RentalUnit>> FindAll()
        {
            var rentalUnits = await this.TenantFilteredEntitiesQuery.Where(x => !x.IsDeleted).OrderBy(x => x.Sort).ToListAsync();
            return rentalUnits;
        }

        public async Task<List<RentalUnit>> FindAllFromAllocationIds(IEnumerable<Guid> rentalUnitIds)
        {
            var rentalUnits = await this.TenantFilteredEntitiesQuery.Where(x => rentalUnitIds.Contains(x.Id)).ToListAsync();
            return rentalUnits;
        }

        public async Task<List<RentalUnit>> FindAllIncludeDeleted()
        {
            var rentalUnits = await this.TenantFilteredEntitiesQuery.OrderBy(x => x.Sort).ToListAsync();
            return rentalUnits;
        }

        public async Task<List<RentalUnit>> FindAllOnline()
        {
            var rentalUnits = await this.TenantFilteredEntitiesQuery.Where(x => !x.IsDeleted && x.IsAvailableOnline).OrderBy(x => x.Sort).ToListAsync();
            return rentalUnits;
        }

        public async Task<List<RentalUnit>> FindAllKiosk()
        {
            var rentalUnits = await this.TenantFilteredEntitiesQuery.Where(x => !x.IsDeleted && ((x.IsAvailableOnline && x.KioskAvailability == BookingAvailability.AsOnline) || (x.KioskAvailability == BookingAvailability.Available))).OrderBy(x => x.Sort).ToListAsync();
            return rentalUnits;
        }
    }
}
