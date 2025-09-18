using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using NxB.Domain.Common.Interfaces;
using NxB.Domain.Common.Model;
using NxB.Dto.AccountingApi;
using NxB.Clients.Interfaces;

namespace NxB.Clients
{
    public class CountryClient : NxBAdministratorClient, ICountryClient
    {
        private readonly Dictionary<string, Country> _countriesCache = new();

        public CountryClient(IHttpContextAccessor httpContextAccessor) : base(httpContextAccessor)
        {
        }

        public async Task<List<Country>> FindAll(bool includeHidden = true)
        {
            var url = $"/NxB.Services.App/NxB.AccountingApi/country/list/all?includeHidden=" + includeHidden;
            var countries = await this.GetAsync<List<Country>>(url);
            return countries;
        }

        public async Task<Country> FindSingle(string countryId)
        {
            if (_countriesCache.ContainsKey(countryId))
            {
                return _countriesCache[countryId];
            }
            var url = $"/NxB.Services.App/NxB.AccountingApi/country?countryId=" + countryId;
            var countryDto = await this.GetAsync<Country>(url);
            this._countriesCache.Add(countryId, countryDto);
            return countryDto;
        }

    }
}
