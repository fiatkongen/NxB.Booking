using NxB.Domain.Common.Model;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace NxB.Clients.Interfaces
{
    public interface ICountryClient: IAuthorizeClient
    {
        Task<List<Country>> FindAll(bool includeHidden = true);
        Task<Country> FindSingle(string countryId);
    }
}
