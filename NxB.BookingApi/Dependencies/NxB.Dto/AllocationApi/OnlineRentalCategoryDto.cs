using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Munk.Utils.Object;

namespace NxB.Dto.AllocationApi
{
    public class OnlineRentalCategoryDto
    {
        [Required]
        [NoEmpty]
        public Guid Id { get; set; }
        public int DepositType { get; set; }
        public decimal? Deposit { get; set; }
        public int Sort { get; set; }
        public int SortOnline { get; set; }
        public bool IsAvailableOnline { get; set; }
        public Guid? OnlineVoucherTemplateId { get; set; }
        public decimal? OnlineSmsAccessCodeHourOffset { get; set; }
        public Guid? OnlineAccessGroupId { get; set; }

        public int? MaxPersons { get; set; }
        public decimal? Height { get; set; }
        public decimal? Width { get; set; }
        public decimal? Depth { get; set; }

        public decimal? DepositPercent { get; set; }
        public decimal? DepositMin { get; set; }
        public decimal? DepositMax { get; set; }

        public decimal? SqmFrom { get; set; }
        public decimal? SqmTo { get; set; }
        public int CheckOutMin { get; set; }
        public int CheckInMin { get; set; }

        public Dictionary<string, string> NameTranslations { get; set; }
        
        public bool AllowSelectResourceOnline { get; set; }
        public Guid? SelectResourceOnlineFee { get; set; }
        public bool DemandConfirmOfResource { get; set; }
    }

    public class ModifyOnlineRentalCategoryDto
    {
        [Required]
        [NoEmpty]
        public Guid Id { get; set; }
        //public int DepositType { get; set; }
        //public decimal? Deposit { get; set; }

        public bool IsAvailableOnline { get; set; }
        public Guid? OnlineVoucherTemplateId { get; set; }
        public decimal? OnlineSmsAccessCodeHourOffset { get; set; }
        public Guid? OnlineAccessGroupId { get; set; }
        public int SortOnline { get; set; }

        public int? MaxPersons { get; set; }
        public decimal? Height { get; set; }
        public decimal? Width { get; set; }
        public decimal? Depth { get; set; }

        public decimal? DepositPercent { get; set; }
        public decimal? DepositMin { get; set; }
        public decimal? DepositMax { get; set; }

        public bool AllowSelectResourceOnline { get; set; }
        public Guid? SelectResourceOnlineFee { get; set; }
        public bool DemandConfirmOfResource { get; set; }
        public int CheckOutMin { get; set; }
        public int CheckInMin { get; set; }
    }
}