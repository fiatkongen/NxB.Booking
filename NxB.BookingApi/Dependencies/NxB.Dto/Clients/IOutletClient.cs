using NxB.Dto.AutomationApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NxB.Dto.Clients
{
    public interface IOutletClient : IAuthorizeClient
    {
        Task<OutletDto> CreateOutlet(CreateOutletDto createOutletDto);
        Task<OutletDto> FindOutlet(Guid id);
        Task<OutletDto> FindOutletFromName(string name);
        Task<List<OutletDto>> FindAllOutlets(bool includeDeleted = false);
        Task<List<OutletDto>> FindAllOutletsFromResourceIds(List<Guid> resourceIds);
        //Task UpdateOutletMeterReading(OutletReadingDto dto);
    }
}
