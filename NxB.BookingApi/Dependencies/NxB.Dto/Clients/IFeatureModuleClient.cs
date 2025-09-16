using Munk.Utils.Object;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NxB.Dto.Clients
{
    public interface IFeatureModuleClient : IAuthorizeClient
    {
        Task<bool> IsFeatureModuleActivatedForTenant(Guid featureModuleId, Guid tenantId);
    }
}
