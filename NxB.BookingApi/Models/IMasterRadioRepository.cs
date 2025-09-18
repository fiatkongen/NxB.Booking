using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NxB.BookingApi.Models
{
    // TODO: Implement MasterRadioRepository interface - placeholder to fix compilation errors
    // This interface was referenced in controllers but not found in the codebase
    public interface IMasterRadioRepository
    {
        // TODO: Implement master radio methods
        Task<List<MasterRadio>> FindAllMasterRadios();
        Task PublishUpdate();
        Task Update(MasterRadio masterRadio);
    }
}