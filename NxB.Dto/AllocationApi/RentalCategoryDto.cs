using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Drawing;
using Munk.Utils.Object;
using NxB.Domain.Common.Dto;
using NxB.Dto.Shared;

namespace NxB.Dto.AllocationApi
{
    public class CreateRentalCategoryDto
    {
        public Dictionary<string, string> NameTranslations { get; set; }
        public Dictionary<string, string> DescriptionTranslations { get; set; }
        public int Sort { get; set; }
        public int SortOnline { get; set; }
        public string Color { get; set; }
        public bool IsAvailableOnline { get; set; }
        public int CheckOutMin { get; set; }
        public int CheckInMin { get; set; }

        [Obsolete]
        public Guid? OnlineDocumentTemplateId { get; set; }
        public Guid? OnlineVoucherTemplateId { get; set; }

        public Guid? PowerMeterArticleId { get; set; }
        public int? MaxPersons { get; set; }
        public decimal? Height { get; set; }
        public decimal? Width { get; set; }
        public decimal? Depth { get; set; }

        public decimal? DepositPercent { get; set; }
        public decimal? DepositMin { get; set; }
        public decimal? DepositMax { get; set; }

        public decimal? SqmFrom { get; set; }
        public decimal? SqmTo { get; set; }

        public bool AllowSelectResourceOnline { get; set; }
        public Guid? SelectResourceOnlineFee { get; set; }
        public bool DemandConfirmOfResource { get; set; }

        public Guid? DefaultPriceProfileId { get; set; }
        public Guid? DefaultOnlinePriceProfileId { get; set; }

        public decimal? Tax { get; set; }

        public BookingAvailability CtoutvertAvailability { get; set; }
        public int? PitchIncludedPersonCtoutvert { get; set; }
        public int? PitchMinPersonCtoutvert { get; set; }
        public int? PitchMaxPersonCtoutvert { get; set; }
        public Guid? OnlineCtoutvertTemplateId { get; set; }
        public Guid? OnlineCtoutvertSmsTemplateId { get; set; }
        public Guid? IncludedPersonsGuestId { get; set; }

        public BookingAvailability KioskAvailability { get; set; }
        public Guid? OnlineKioskTemplateId { get; set; }
        public Guid? OnlineKioskSmsTemplateId { get; set; }
        public Guid? KioskAccessGroupId { get; set; }

        public string IconName { get; set; }
    }

    public class RentalCategoryDto : CreateRentalCategoryDto, ICategoryDto
    {
        [Required]
        [NoEmpty]
        public Guid Id { get; set; }
        public long LegacyId { get; set; }
        public int DepositType { get; set; }
        public decimal? Deposit { get; set; }
        public bool IsDeleted { get; set; }

        public decimal? OnlineSmsAccessCodeHourOffset { get; set; }
        public Guid? OnlineAccessGroupId { get; set; }
    }

    public class ModifyRentalCategoryDto : CreateRentalCategoryDto
    {
        [Required]
        [NoEmpty]
        public Guid Id { get; set; }
    }

    //converted to extension because of serialization
    public static class RentalCategoryDtoExtensions
    {
        public static bool IsMarkedForCtoutvertUpdate(this RentalCategoryDto rentalCategoryDto) => !rentalCategoryDto.IsDeleted &&
                                                           ((rentalCategoryDto.IsAvailableOnline && rentalCategoryDto.CtoutvertAvailability ==
                                                                   BookingAvailability.AsOnline) || rentalCategoryDto.CtoutvertAvailability ==
                                                               BookingAvailability.Available);
    }
}
