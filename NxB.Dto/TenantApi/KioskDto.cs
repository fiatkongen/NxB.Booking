using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Munk.Utils.Object;

namespace NxB.Dto.TenantApi
{
    public class CreateKioskDto
    {
        [NoEmpty]
        public string Name { get; set; }

        [NoEmpty]
        public string HardwareSerialNo { get; set; }

        public KioskState State { get; set; }
        public int? BlankScreenMinutesStart { get; set; }
        public int? BlankScreenMinutesEnd { get; set; }
        public BlankScreenOption BlankScreenOption { get; set; }
    }

    public class KioskDto : CreateKioskDto
    {
        public Guid Id { get; set; }
        public Guid TenantId { get; set; }
        public DateTime? LastOnline { get; set; }
        public KioskState LastState { get; set; }
        public bool IsDeleted { get; set; }
    }

    public class ModifyKioskDto : CreateKioskDto
    {
        public Guid Id { get; set; }
    }

    public enum KioskState
    {
        Offline,
        Online,
        OutOfOrder,
    }

    public enum BlankScreenOption
    {
        AlwaysOn,
        Blank,
        PauseScreen,
    }
}
