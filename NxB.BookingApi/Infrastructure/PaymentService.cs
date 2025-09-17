using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Internal;
using NxB.BookingApi.Exceptions;
using NxB.BookingApi.Models;

namespace NxB.BookingApi.Infrastructure
{
    public class PaymentService : IPaymentService
    {
        private readonly IVoucherRepository _voucherRepository;
        private readonly IFriendlyAccountingIdProvider _friendlyAccountingIdProvider;

        public PaymentService(IVoucherRepository voucherRepository, IFriendlyAccountingIdProvider friendlyAccountingIdProvider)
        {
            _voucherRepository = voucherRepository;
            _friendlyAccountingIdProvider = friendlyAccountingIdProvider;
        }

        public Payment Credit(Guid id)
        {
            var payment = _voucherRepository.FindSinglePayment(id);

            var creditedPayment = payment.Revert(_friendlyAccountingIdProvider);
            
            creditedPayment.DocumentId = Guid.NewGuid();

            if (!payment.IsClosed)
            {
                Guid voucherCloseTransactionid = Guid.NewGuid();
                creditedPayment.Close(voucherCloseTransactionid);
                payment.Close(voucherCloseTransactionid);
                creditedPayment.Close(voucherCloseTransactionid);
            }

            return creditedPayment;
        }
    }
}