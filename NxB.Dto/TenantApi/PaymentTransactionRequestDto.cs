using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NxB.Dto.TenantApi
{
    public class PaymentTransactionRequestDto
    {
        public string TerminalName { get; set; }
        public decimal Amount { get; set; }
        public Guid VoucherId { get; set; }
        public string SaleId { get; set; }
    }
}
