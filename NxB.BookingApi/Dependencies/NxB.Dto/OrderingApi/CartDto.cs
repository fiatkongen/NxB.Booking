using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Munk.Utils.Object;
using NxB.Domain.Common.Enums;

namespace NxB.Dto.OrderingApi
{
    public class CartDto
    {
        public OnlineCustomerDto Customer { get; set; }

        [NoEmpty]
        public Guid OrderId { get; set; }

        public long QuickPayOrderId { get; set; }

        [NoEmpty]
        public Guid TenantId { get; set; }
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public DateTime ArrivalTime { get; set; }
        public List<BookingCartItemDto> BookingCartItems { get; set; }
        public decimal ValidateTotal { get; set; }
        public decimal Deposit { get; set; }
        public string OnlineCreationErrors { get; set; }
        public string Note { get; set; }
        public bool NoteState { get; set; }
        public string OnlineTransactionDetails { get; set; }
        public CreatedBy CreatedBy { get; set; } = CreatedBy.OnlineBooking;
        public string CreatedByExternalId { get; set; }
        public string ExternalId { get; set; }

        public string ExternalPaymentType { get; set; }
        public decimal? ExternalPaymentAmount { get; set; }

    }

    public class BookingCartItemDto
    {
        [NoEmpty]
        public Guid RentalCategoryId { get; set; }

        [Required(AllowEmptyStrings = false)]
        public string RentalCategoryName { get; set; }

        public Guid? RentalUnitId { get; set; }

        public string RentalUnitName { get; set; }

        [NoEmpty]
        public Guid PriceProfileId { get; set; }

        [Required(AllowEmptyStrings = false)]
        public string PriceProfileName { get; set; }

        public int Number { get; set; } = 1;
        public decimal PriceValidate { get; set; }
        public decimal TotalValidate { get; set; }
        public decimal Deposit { get; set; }
        public bool IsCustomPricePcs { get; set; }

        public List<GuestUnitCartItemDto> GuestUnitCartItems { get; set; }
        public List<ArticleUnitCartItemDto> ArticleUnitCartItems { get; set; }

        public Guid? RentalSubTypeId { get; set; }
        public string RentalSubTypeName { get; set; }
    }

    public class UnitCartItemDto
    {
        [NoEmpty]
        public Guid Id { get; set; }

        [Required(AllowEmptyStrings = false)]
        public string TypeName { get; set; }

        public int Number { get; set; }
        
        [NoEmpty]
        public Guid PriceProfileId { get; set; }

        [Required(AllowEmptyStrings = false)]
        public string PriceProfileName { get; set; }

        public decimal PriceValidate { get; set; }
        public decimal TotalValidate { get; set; }

        public bool IsCustomPricePcs { get; set; }
    }

    public class GuestUnitCartItemDto : UnitCartItemDto
    {
    }

    public class ArticleUnitCartItemDto : UnitCartItemDto
    {
    }



}
