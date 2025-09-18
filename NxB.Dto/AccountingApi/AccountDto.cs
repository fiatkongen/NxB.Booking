using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using Munk.Utils.Object;
using NxB.Domain.Common.Interfaces;

namespace NxB.Dto.AccountingApi
{
    public class CreateCustomerAccountDto
    {
        [Required(AllowEmptyStrings = false)]
        [MaxLength(100)]
        public string Name { get; set; }
    }

    public class CreateAccountDto : CreateCustomerAccountDto
    {
        [Required]
        [NoEmpty]
        public Guid CustomerId { get; set; }
    }

    public class AccountDto: CreateAccountDto, IAccountKey
    {
        [Required]
        [NoEmpty]
        public Guid Id { get; set; }
        public int Index { get; set; }
        public string FriendlyId { get; set; }
        public bool IsDeleted { get; set; }
    }
}
