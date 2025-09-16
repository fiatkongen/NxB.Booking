using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Munk.AspNetCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using NxB.Domain.Common.Interfaces;
using NxB.BookingApi.Models;


namespace NxB.BookingApi.Infrastructure
{
    public class AllocationStateRepository : TenantFilteredRepository<AllocationState, AppDbContext>, IAllocationStateRepository
    {
        private List<AllocationState> _notCommitted = new();

        public AllocationStateRepository(IClaimsProvider claimsProvider, AppDbContext appDbContext) : base(claimsProvider, appDbContext)
        {
        }

        public IAllocationStateRepository CloneWithCustomClaimsProvider(IClaimsProvider overrideClaimsProvider)
        {
            return new AllocationStateRepository(overrideClaimsProvider, AppDbContext);
        }


        public void Add(AllocationState allocationState)
        {
            Serialize(allocationState);
            this.AppDbContext.AllocationStates.Add(allocationState);
            this._notCommitted.Add(allocationState);
        }

        public AllocationState FindSingle(Guid subOrderId)
        {
            var allocationState = this.AppDbContext.AllocationStates.Single(x => x.SubOrderId == subOrderId);
            Deserialize(allocationState);
            return allocationState;
        }

        public AllocationState FindSingleOrDefault(Guid subOrderId)
        {
            var allocationState = this.AppDbContext.AllocationStates.SingleOrDefault(x => x.SubOrderId == subOrderId);
            if (allocationState!= null)
            {
                Deserialize(allocationState);
            }

            return allocationState;
        }

        public AllocationState FindSingleOrDefaultNotCommitted(Guid subOrderId)
        {
            var allocationState = this._notCommitted.SingleOrDefault(x => x.SubOrderId == subOrderId);
            
            return allocationState;
        }

        public void Update(AllocationState allocationState)
        {
            Serialize(allocationState);
            this.AppDbContext.AllocationStates.Update(allocationState);
        }

        private void Deserialize(AllocationState allocationState)
        {
            if (string.IsNullOrEmpty(allocationState.LogsJson)) return;;
            var states = JsonConvert.DeserializeObject<(List<AllocationStateLog>, List<AllocationStateLog>)>(allocationState.LogsJson);
            allocationState.ArrivalStateLogs = states.Item1;
            allocationState.DepartureStateLogs = states.Item2;
        }

        public static void Serialize(AllocationState allocationState)
        {
            allocationState.LogsJson = JsonConvert.SerializeObject((allocationState.ArrivalStateLogs, allocationState.DepartureStateLogs), new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            });
        }
    }
}
