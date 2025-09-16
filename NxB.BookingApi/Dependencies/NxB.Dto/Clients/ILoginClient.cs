using System;
using System.Threading.Tasks;
using NxB.Dto.LoginApi;

namespace NxB.Dto.Clients
{
    public interface ILoginClient
    {
        Task<Guid> CreateSession(LoginDto loginDto);
    }
}
