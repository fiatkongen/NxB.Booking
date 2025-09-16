using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;
using Munk.Utils.Object;
using NxB.Domain.Common.Dto;

namespace NxB.Dto.AllocationApi
{
    public class BaseRentalUnitDto
    {
        public int Sort { get; set; }
        public decimal? WidthMeters { get; set; }
        public decimal? HeightMeters { get; set; }
        public bool IsPowered { get; set; }
        public decimal? PowerMeter { get; set; }
        public Dictionary<string, string> NameTranslations { get; set; }
        public Dictionary<string, string> DescriptionTranslations { get; set; }
        public bool IsAvailableOnline { get; set; }
        public int? TallySocketRadioId { get; set; }
        public int? TallySocketNo { get; set; }
        public Guid? LinkAccessId { get; set; }
        public BookingAvailability KioskAvailability { get; set; }
        public BookingAvailability CtoutvertAvailability { get; set; }

        [Required]
        [NoEmpty]
        public Guid RentalCategoryId { get; set; }
    }

    public class CreateRentalUnitDto : BaseRentalUnitDto
    {

    }

    public class RentalUnitDto : CreateRentalUnitDto
    {
        [Required]
        [NoEmpty]
        public Guid Id { get; set; }
        public long LegacyId { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime? MeterLastModified { get; set; }
    }

    public class RentalUnitOnlineDto : RentalUnitDto
    {
        public MapSymbolDto MapSymbol { get; set; }
    }

    public class ModifyRentalUnitDto : BaseRentalUnitDto
    {
        public Guid Id { get; set; }
    }
}
