using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NxB.Dto.AllocationApi;

namespace NxB.Dto.Clients
{
    public interface IRentalSubTypeClient
    {
        Task<RentalSubTypeDto> FindSingleOrDefault(Guid id);
    }
}
