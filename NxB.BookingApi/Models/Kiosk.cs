using NxB.Domain.Common.Dto;
using NxB.Domain.Common.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NxB.Dto.TenantApi;

namespace NxB.BookingApi.Models
{
    [Serializable]
    public class Kiosk : ITenantEntity
    {
        public Guid Id { get; set; }
        public Guid TenantId { get; set; }
        public string Name { get; set; }
        public string HardwareSerialNo { get; set; }
        public DateTime? LastOnline { get; set; }
        public KioskState LastState { get; set; }
        public KioskState State { get; set; }
        public int? BlankScreenMinutesStart { get; set; }
        public int? BlankScreenMinutesEnd { get; set; }
        public BlankScreenOption BlankScreenOption { get; set; }
        public bool IsDeleted { get; set; }

        public void UpdateOnlineStatus()
        {
            LastOnline = DateTime.Now.ToEuTimeZone();
            LastState = State;
            State = KioskState.Online;
        }
    }
}