using System;
using System.Collections.Generic;
using System.Text;
using NxB.Dto.AccountingApi;

namespace NxB.Dto.DocumentApi
{
    public class CreateVoucherPdfDto
    {
        public ReadVoucherDto VoucherDto { get; set; }
        public PaymentDto DefaultPaymentDto { get; set; }
        public string OverrideDocumentText { get; set; }
    }
}
