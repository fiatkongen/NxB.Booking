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
    public class KioskRepository : TenantFilteredRepository<Kiosk, AppDbContext>, IKioskRepository
    {
        public KioskRepository(IClaimsProvider claimsProvider, AppDbContext appDbContext) : base(claimsProvider, appDbContext)
        {
        }

        public void Add(Kiosk kiosk)
        {
            AppDbContext.Add(kiosk);
        }

        public void Update(Kiosk kiosk)
        {
            AppDbContext.Update(kiosk);
        }

        public void MarkAsDeleted(Kiosk kiosk)
        {
            kiosk.IsDeleted = true;
        }

        public void MarkAsUndeleted(Kiosk kiosk)
        {
            kiosk.IsDeleted = false;
        }

        public Task<List<Kiosk>> FindAll(bool includeDeleted)
        {
            return TenantFilteredEntitiesQuery.Where(x => includeDeleted || !x.IsDeleted).ToListAsync();
        }

        public Task<Kiosk> FindSingleOrDefault(Guid id)
        {
            return AppDbContext.Kiosks.SingleOrDefaultAsync(x => x.Id == id);
        }

        public Task<Kiosk> FindSingleOrDefaultFromHardwareSerialNo(string serialNo)
        {
            return AppDbContext.Kiosks.SingleOrDefaultAsync(x => x.HardwareSerialNo == serialNo);
        }
    }
}