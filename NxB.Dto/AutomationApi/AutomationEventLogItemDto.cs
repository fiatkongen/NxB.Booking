using Munk.Utils.Object;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ServiceStack.Text;

namespace NxB.Dto.AutomationApi
{
    public class AutomationEventLogItemDto
    {
        public int? RowId { get; set; }
        public DateTime TimeStamp { get; set; }
        public string EventType { get; set; }
        public string EventName { get; set; }
        public string Sender { get; set; }
        public string ReservationId { get; set; }
        public string CustomerName { get; set; }
        public Guid? OrderId { get; set; }
        public int? FriendlyOrderId { get; set; }
        public string RentalUnitName { get; set; }
        public Guid TenantId { get; set; }
        public DateTime? Start { get; set; }
        public DateTime? End { get; set; }
        public Guid? SubOrderId => Guid.TryParse(ReservationId, out var noUse) ? Guid.Parse(ReservationId) : null;
        
        public string Data { get; set; }
        public LicensePlateData LicensePlateData () => string.IsNullOrEmpty(Data) ? null : JsonConvert.DeserializeObject<LicensePlateData>(Data);

        public static AutomationEventLogItemDto Create(dynamic logItemRaw, Guid tenantId)
        {

            var dataString = logItemRaw.data != null ? logItemRaw.data.ToString(Formatting.None) : "{}";
            dynamic data = logItemRaw.data;
            var licensePlateAccessDto = new AutomationEventLogItemDto
            {
                RowId = logItemRaw.rowid,
                TimeStamp = ((long)logItemRaw.timestamp).ToEuTimeZoneFromUnixMs(),
                EventType = logItemRaw.eventtype,
                EventName = logItemRaw.eventname,
                Sender = logItemRaw.sender,
                Data = dataString,
                TenantId = tenantId,
//                Start = data.startdate != null ? ((long)data.startdate).FromUnixTimeMs().ToEuTimeZoneFromUtc() : null,
//                End = data.enddate != null ? ((long)data.enddate).FromUnixTimeMs().ToEuTimeZoneFromUtc() : null,
                ReservationId = dataString != null && dataString.Contains("DOCTYPE") ? null : data?.reservation?.id
            };
            return licensePlateAccessDto;
        }
    }

    public class LicensePlateData
    {
        public string Licenseplate { get; set; }
        public string Denyreason { get; set; }
    }
}
