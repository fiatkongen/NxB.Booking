using System;
using System.Threading.Tasks;

namespace NxB.Clients.Interfaces
{
    public interface IAuthorizeClient
    {
        Task AuthorizeClient(Guid? tenantId = null);
        Task TrySignOutClient();
    }
}
