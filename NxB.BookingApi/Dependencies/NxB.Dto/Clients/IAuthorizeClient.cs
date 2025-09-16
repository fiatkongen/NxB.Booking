using System;
using System.Threading.Tasks;

namespace NxB.Dto.Clients
{
    public interface IAuthorizeClient
    {
        Task AuthorizeClient(Guid? tenantId = null);
        Task TrySignOutClient();
    }
}
