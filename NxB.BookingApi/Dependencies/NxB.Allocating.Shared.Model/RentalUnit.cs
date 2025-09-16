using System;
using NxB.Domain.Common.Dto;
using NxB.Domain.Common.Interfaces;
using NxB.Domain.Common.Model;

namespace NxB.Allocating.Shared.Model
{
    [Serializable]
    public class RentalUnit : BaseTranslatedEntity, ITenantEntity
    {
        public Guid Id { get; set; }
        public Guid TenantId { get; set; }
        public long LegacyId { get; set; }
        public int Sort { get; set; }
        public decimal? WidthMeters { get; set; }
        public decimal? HeightMeters { get; set; }
        public bool IsPowered { get; set; }
        public bool IsDeleted { get; set; }
        public bool IsImported { get; set; }
        public long RentalCategoryLegacyId { get; set; }
        public Guid RentalCategoryId { get; set; }
        public decimal PowerMeter { get; private set; }
        public DateTime? MeterLastModified { get; set; }
        public bool IsAvailableOnline { get; set; }
        public int? TallySocketRadioId { get; set; }
        public int? TallySocketNo { get; set; }
        public Guid? LinkAccessId { get; set; }
        public BookingAvailability KioskAvailability { get; set; }
        public BookingAvailability CtoutvertAvailability { get; set; }

        public string Name
        {
            get => this.NameTranslator.GetTranslation("s", "da");
            set => AddDefaultNameTranslation(value);
        }

        private RentalUnit(Guid id, Guid tenantId, long legacyId, Guid rentalCategoryId, long rentalCategoryLegacyId, string nameTranslationsJson = null, string descriptionTranslationsJson = null) : base(nameTranslationsJson, descriptionTranslationsJson)
        {
            Id = id;
            TenantId = tenantId;
            LegacyId = legacyId;
            RentalCategoryId = rentalCategoryId;
            RentalCategoryLegacyId = rentalCategoryLegacyId;
        }

        //For now, only a constructor exists that demands a legacyId.
        public RentalUnit(Guid id, string name, Guid tenantId, long legacyId, Guid rentalCategoryId, long rentalCategoryLegacyId, string nameTranslationsJson = null, string descriptionTranslationsJson = null) : base(nameTranslationsJson, descriptionTranslationsJson)
        {
            this.AddDefaultNameTranslation(name);
            Id = id;
            TenantId = tenantId;
            LegacyId = legacyId;
            RentalCategoryId = rentalCategoryId;
            RentalCategoryLegacyId = rentalCategoryLegacyId;
        }

        public void SetPowerMeter(decimal powerMeter)
        {
            this.PowerMeter = powerMeter;
            this.MeterLastModified = DateTime.Now.ToEuTimeZone();
        }
    }
}

