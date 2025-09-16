using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using NxB.Dto.AllocationApi;

namespace NxB.Dto.Clients
{
    public interface IRentalUnitClient : IAuthorizeClient
    {
        Task<List<RentalUnitDto>> FindAll(bool includeDeleted);
        Task<RentalUnitDto> FindSingleOrDefault(Guid id);
    }

    public interface IRentalUnitClientCached : IRentalUnitClient { }
}
