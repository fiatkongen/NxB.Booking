using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Munk.Utils.Object;

namespace NxB.Dto.AutomationApi
{
    public class CreateOutletDto
    {
        public string Name { get; set; }
        public string MeterName { get; set; }
        public decimal Meter { get; set; }
        public bool IsOn { get; set; }
        public bool IsDisabled { get; set; }
        public Guid? OutletHubId { get; set; }
    }

    public class OutletDto : CreateOutletDto
    {
        public Guid Id { get; set; }
        public Guid TenantId { get; set; }
        public Guid? RentalUnitId { get; set; }
        public int? ExternalSwitchId { get; set; }
        public int? ExternalMeterId { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime LastMeterRead { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime? DisableDate { get; set; }
        public DateTime? DeleteDate { get; set; }
        public Guid CreatedBy { get; set; }
        public Guid? DeletedBy { get; set; }
        public Guid? DisabledBy { get; set; }

        public string OutletHubName { get; set; }
        public int? OutletHubExternalId { get; set; }
    }

    public class OutletReadingDto
    {
        public Guid? OutletId { get; set; }
        public string OutletName { get; set; }
        public decimal Meter { get; set; }
        public DateTime? ReadingTime { get; set; }
    }

    public class ModifyRentalUnitLinkDto
    {
        [NoEmpty]
        public Guid OutletId { get; set; }
        public Guid? RentalUnitId { get; set; }
    }

    public class AuningOutletReadingDto
    {
        public string SwitchAddress { get; set; }
        public int UnitNo { get; set; }
        public decimal Kwh { get; set; }
        public DateTime LastUpdateDate { get; set; }
        public bool IsEnabled { get; set; }
    }

    public class GuestOutletDto
    {
        public Guid Id { get; set; }
        public Guid? RentalUnitId { get; set; }
        public int? ExternalSwitchId { get; set; }
        public int? ExternalMeterId { get; set; }
        public DateTime LastMeterRead { get; set; }
        public string OutletHubName { get; set; }
        public int? OutletHubExternalId { get; set; }
        public string Name { get; set; }
        public string MeterName { get; set; }
        public decimal Meter { get; set; }
        public bool IsOn { get; set; }
        public Guid? OutletHubId { get; set; }
        public string RentalUnitName { get; set; }
    }
}
