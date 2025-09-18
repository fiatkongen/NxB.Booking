using System;
using System.Threading.Tasks;
using NxB.Dto.LoginApi;

namespace NxB.Clients.Interfaces
{
    public interface ILoginClient
    {
        Task<Guid> CreateSession(LoginDto loginDto);
    }
}
