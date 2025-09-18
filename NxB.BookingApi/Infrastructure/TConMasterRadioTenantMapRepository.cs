using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Munk.AspNetCore;
using NxB.Domain.Common.Interfaces;
using NxB.BookingApi.Models;

namespace NxB.BookingApi.Infrastructure
{
    public class TConMasterRadioTenantMapRepository : TenantFilteredRepository<TConMasterRadioTenantMap, AppDbContext>, ITConMasterRadioTenantMapRepository
    {
        private readonly IClaimsProvider _claimsProvider;
        private readonly AppDbContext _appDbContext;
        private int? _masterRadioId;

        public int MasterRadioId
        {
            get
            {
                if (!_masterRadioId.HasValue)
                {
                    _masterRadioId = FindAllMasterRadioMappings()?.TallyMasterRadioId ?? 0;
                }
                return _masterRadioId.Value;
            }
        }

        public TConMasterRadioTenantMapRepository(IClaimsProvider claimsProvider, AppDbContext appDbContext) : base(claimsProvider, appDbContext)
        {
            _claimsProvider = claimsProvider;
            _appDbContext = appDbContext;
        }


        public IMasterRadioIdProvider CloneWithCustomClaimsProvider(IClaimsProvider overrideClaimsProvider)
        {
            return new TConMasterRadioTenantMapRepository(overrideClaimsProvider ?? _claimsProvider, _appDbContext);
        }

        public void Add(TConMasterRadioTenantMap conMasterRadioTenantMap)
        {
            AppDbContext.Add(conMasterRadioTenantMap);
        }

        public void Delete(Guid id)
        {
            throw new NotImplementedException();
        }

        public TConMasterRadioTenantMap FindAllMasterRadioMappings()
        {
            return this.TenantFilteredEntitiesQuery.SingleOrDefault(x => x.TenantId == _claimsProvider.GetTenantId());
        }

    }
}