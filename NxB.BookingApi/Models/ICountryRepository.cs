using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using NxB.Domain.Common.Model;

namespace NxB.BookingApi.Models
{
    public interface ICountryRepository
    {
        Task<List<Country>> FindAll(bool includeHidden = true);
        void Update(Country country);
        Task<Country> FindSingle(string id);
    }
}
