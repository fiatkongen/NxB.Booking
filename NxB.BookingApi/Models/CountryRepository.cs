using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NxB.BookingApi.Infrastructure;
using NxB.Domain.Common.Model;

namespace NxB.BookingApi.Models
{
    public class CountryRepository : ICountryRepository
    {
        protected AppDbContext AppDbContext { get; private set; }

        public CountryRepository(AppDbContext appDbContext)
        {
            AppDbContext = appDbContext;
        }

        public async Task<List<Country>> FindAll(bool includeHidden = true)
        {
            var countries = await this.AppDbContext.Countries.Where(x => includeHidden || !x.IsHidden).ToListAsync();
            foreach (var country in countries)
            {
                country.Deserialize();
            }

            return countries;
        }

        public void Update(Country country)
        {
            country.Serialize();
        }

        public Task<Country> FindSingle(string id)
        {
            return this.AppDbContext.Countries.SingleAsync(x => x.Id == id);
        }
    }
}
