using System;
using System.Collections.Generic;
using System.Text;
using NxB.Domain.Common.Enums;

namespace NxB.Domain.Common.Model
{
    public static class VoucherTypeConverter
    {
        public static VoucherType ToVoucherType(VoucherTemplateType voucherTemplateType)
        {
            switch (voucherTemplateType)
            {
                case VoucherTemplateType.None:
                    return VoucherType.None;
                case VoucherTemplateType.Invoice:
                    return VoucherType.Invoice;
                case VoucherTemplateType.CreditNote:
                    return VoucherType.CreditNote;
                case VoucherTemplateType.Deposit:
                    return VoucherType.Deposit;
                case VoucherTemplateType.Payment:
                    return VoucherType.Payment;
                default:
                    throw new ArgumentOutOfRangeException(nameof(voucherTemplateType), voucherTemplateType, null);
            }
        }

        public static VoucherTemplateType ToVoucherTemplateType(VoucherType voucherType)
        {
            switch (voucherType)
            {
                case VoucherType.None:
                    return VoucherTemplateType.None;
                case VoucherType.Invoice:
                    return VoucherTemplateType.Invoice;
                case VoucherType.CreditNote:
                    return VoucherTemplateType.CreditNote;
                case VoucherType.Payment:
                    return VoucherTemplateType.Payment;
                case VoucherType.Deposit:
                    return VoucherTemplateType.Deposit;
                default:
                    throw new ArgumentOutOfRangeException(nameof(voucherType), voucherType, null);
            }
        }
    }
}
