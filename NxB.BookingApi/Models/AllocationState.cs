using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Munk.Utils.Object;
using Newtonsoft.Json;
using NxB.Domain.Common.Enums;
using NxB.Domain.Common.Interfaces;

namespace NxB.BookingApi.Models
{
    [Serializable]
    public class AllocationStateLog
    {
        public Guid CreateAuthorId { get; set; }
        public DateTime CreateDate { get; set; }
        public AllocationStatus? Status { get; set; } 
        public DateTime? CustomTime { get; set; } 
        public string Text { get; set; }

        [JsonConstructor]
        private AllocationStateLog()
        {
        }

        public AllocationStateLog(Guid createAuthorId, AllocationStatus? status, DateTime? customTime, string text)
        {
            CreateAuthorId = createAuthorId;
            CreateDate =  DateTime.Now.ToEuTimeZone();
            Status = status;
            CustomTime = customTime;
            Text = text;
        }
    }

    [Serializable]
    public class AllocationState : ITenantEntity
    {
        public Guid SubOrderId { get; private set; }
        public Guid TenantId { get; set; }

        public ArrivalStatus ArrivalStatus { get; set; }
        public DateTime? ArrivalTime { get; set; }
        public string ArrivalText => String.Join("~", ArrivalStateLogs.Where(x => !string.IsNullOrEmpty(x.Text)).Select(x => x.Text));

        public DepartureStatus DepartureStatus { get; set; }
        public DateTime? DepartureTime { get; private set; }
        public string DepartureText => String.Join("~", DepartureStateLogs.Where(x => !string.IsNullOrEmpty(x.Text)).Select(x => x.Text));

        public List<AllocationStateLog> ArrivalStateLogs { get; set; } = new();
        public List<AllocationStateLog> DepartureStateLogs { get; set; } = new();
        public string LogsJson { get; set; }

        [JsonConstructor]
        private AllocationState()
        {
        }

        internal AllocationState(Guid subOrderId, Guid createAuthorId, Guid tenantId)
        {
            SubOrderId = subOrderId;
            TenantId = tenantId;
            AddArrivalLog(createAuthorId, ArrivalStatus.NotArrived, null, null);
            AddDepartureLog(createAuthorId, DepartureStatus.NotDeparted, null, null);
        }

        public void AddArrivalLog(Guid createAuthorId, ArrivalStatus? status, DateTime? customTime, string text)
        {
            var log = new AllocationStateLog(createAuthorId, status.HasValue ? (AllocationStatus?)status.Value : null, customTime, text);
            ArrivalStateLogs.Add(log);
            if (status.HasValue) ArrivalStatus = status.Value;
            if (customTime.HasValue) ArrivalTime = customTime.Value;

            if (status.HasValue && status.Value == ArrivalStatus.Cancelled)
            {
                ArrivalTime = null;
            }
        }

        public void AddDepartureLog(Guid createAuthorId, DepartureStatus? status, DateTime? customTime, string text)
        {
            var log = new AllocationStateLog(createAuthorId, status.HasValue ? (AllocationStatus?) status.Value : null, customTime, text);
            DepartureStateLogs.Add(log);
            if (status.HasValue) DepartureStatus = status.Value;
            if (customTime.HasValue) DepartureTime = customTime.Value;

            if (status.HasValue && status.Value == DepartureStatus.Cancelled)
            {
                DepartureTime = null;
            }
        }
    }
}
