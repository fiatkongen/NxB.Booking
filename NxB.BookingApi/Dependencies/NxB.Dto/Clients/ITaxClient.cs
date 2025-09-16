using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NxB.Dto.Clients
{
    public interface ITaxClient : IAuthorizeClient
    {
        public Task<Dictionary<Guid, decimal>> MapTaxesFromResources(List<Guid> resourceIds);
    }
}
