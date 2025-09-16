using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using Munk.Utils.Object;

namespace NxB.Dto.AccountingApi
{
    public class CreateCustomerGroupDto
    {
        [Required(AllowEmptyStrings = false)]
        [MaxLength(100)]
        public string Name { get; set; }
    }

    public class CustomerGroupDto : CreateCustomerGroupDto
    {
        [Required]
        [NoEmpty]
        public Guid Id { get; set; }
        public bool IsDeleted { get; set; }
    }
}
