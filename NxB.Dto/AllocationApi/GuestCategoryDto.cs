using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Munk.Utils.Object;
using NxB.Domain.Common.Dto;

namespace NxB.Dto.AllocationApi
{
    public class BaseGuestCategoryDto
    {
        public Dictionary<string, string> NameTranslations { get; set; }
        public Dictionary<string, string> DescriptionTranslations { get; set; }
        public int Sort { get; set; }
        public List<GuestRentalCategoryDto> GuestRentalCategories { get; set; }
        public bool IsAvailableOnline { get; set; }
        public BookingAvailability KioskAvailability { get; set; }
        public int PersonCount { get; set; }

        public decimal? Tax { get; set; }
    }

    public class CreateGuestCategoryDto : BaseGuestCategoryDto
    {
        public Guid? DefaultPriceProfileId { get; set; }
        public Guid? DefaultOnlinePriceProfileId { get; set; }
    }

    public class GuestCategoryDto : CreateGuestCategoryDto, ICategoryDto
    {
        [Required]
        [NoEmpty]
        public Guid Id { get; set; }
        public long LegacyId { get; set; }

        public long? DefaultLegacyPriceProfileId { get; set; }
        public int DepositType { get; set; }
        public decimal? Deposit { get; set; }
        public bool IsDeleted { get; set; }

    }

    public class ModifyGuestCategoryDto : BaseGuestCategoryDto
    {
        [Required]
        [NoEmpty]
        public Guid Id { get; set; }
    }
}