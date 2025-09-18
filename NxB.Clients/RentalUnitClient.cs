using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using NxB.Domain.Common.Interfaces;
using NxB.Dto.AllocationApi;
using NxB.Clients.Interfaces;

namespace NxB.Clients
{
    public class RentalUnitClient : NxBAdministratorClient, IRentalUnitClient
    {
        public RentalUnitClient(IHttpContextAccessor httpContextAccessor) : base(httpContextAccessor)
        {
        }

        public virtual async Task<List<RentalUnitDto>> FindAll(bool includeDeleted)
        {
            var url = $"/NxB.Services.App/NxB.AllocationApi/rentalunit/list/all?includeDeleted=" + includeDeleted;
            var rentalUnitDtos = await this.GetAsync<List<RentalUnitDto>>(url);
            return rentalUnitDtos;
        }

        public virtual async Task<RentalUnitDto> FindSingleOrDefault(Guid id)
        {
            var url = $"/NxB.Services.App/NxB.AllocationApi/rentalunit?id=" + id;
            var rentalUnitDto = await this.GetAsync<RentalUnitDto>(url);
            return rentalUnitDto;
        }
    }

    public class RentalUnitClientCached : RentalUnitClient, IRentalUnitClientCached
    {
        private readonly List<RentalUnitDto> _cache = new();

        public RentalUnitClientCached(IHttpContextAccessor httpContextAccessor) : base(httpContextAccessor)
        {
        }

        public override async Task<List<RentalUnitDto>> FindAll(bool includeDeleted)
        {
            var rentalUnits = await base.FindAll(includeDeleted);
            _cache.AddRange(rentalUnits);
            return rentalUnits;
        }

        public override async Task<RentalUnitDto> FindSingleOrDefault(Guid id)
        {
            var item = _cache.SingleOrDefault(x => x.Id == id);
            if (item == null)
            {
                item = await base.FindSingleOrDefault(id);
                if (item != null)
                {
                    _cache.Add(item);
                }
            }
            return item;
        }
    }
}
