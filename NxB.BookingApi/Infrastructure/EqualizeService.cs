using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Munk.Utils.Object;
using NxB.BookingApi.Exceptions;
using NxB.BookingApi.Models;

namespace NxB.BookingApi.Infrastructure
{
    public class EqualizeService : IEqualizeService
    {
        private readonly IVoucherRepository _voucherRepository;

        public EqualizeService(IVoucherRepository voucherRepository)
        {
            _voucherRepository = voucherRepository;
        }

        public async Task<bool> TryEqualizeInvoiceWithSpecificPayment(Payment newPayment, Guid invoiceId,
            IInvoiceService invoiceService, Guid closeTransactionId)
        {
            try
            {
                var existingPayments = await _voucherRepository.FindSpecificPaymentsFromInvoiceId(invoiceId, false);
                var existingPaymentIds = existingPayments.Where(x => x != newPayment).Select(x => x.Id).ToList();

                await EqualizeVouchersAndPayments(newPayment, existingPaymentIds, new List<Guid> { invoiceId },
                    invoiceService, closeTransactionId);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task EqualizeVouchersAndPayments(Payment newPayment,
            List<Guid> existingPaymentIds, List<Guid> invoiceIds, IInvoiceService invoiceService,
            Guid closeTransactionId)
        {
            if (newPayment != null || existingPaymentIds.Count > 0 || invoiceIds.Count > 1)
            {
                //throw exception if trying to pay both deposits AND invoices
                var existingPayments = await _voucherRepository.FindPaymentsFromIds(existingPaymentIds, false);
                var invoices = await _voucherRepository.FindInvoiceBasesFromIds<InvoiceBase>(invoiceIds, false);
                var dueDeposits = await _voucherRepository.FindVouchersFromIds<DueDeposit>(invoiceIds, false);

                if (dueDeposits.Count > 0 && invoices.Count > 0)
                {
                    throw new VoucherException("Kan ikke behandle betaling af fakturaer og bekræftelser på samme tid");
                }
                if (invoices.Count > 0)
                {
                    EqualizeInvoicesAndPayments(newPayment, existingPayments, invoices, closeTransactionId);
                }
                else if (dueDeposits.Count > 0)
                {
                    CloseDepositsFromMatchingPayments(newPayment, existingPayments, dueDeposits, closeTransactionId);
                }
                else if (existingPaymentIds.Count > 1 && dueDeposits.Count == 0 && invoices.Count == 0)
                {
                    CloseEqualizingPayments(existingPayments, closeTransactionId);
                }
            }
        }

        private void CloseDepositsFromMatchingPayments(Payment newPayment,
            List<Payment> existingPayments, List<DueDeposit> deposits,
            Guid closeTransactionId)
        {
            var totalPaymentAmount = 0 - ((newPayment?.Total ?? 0) + existingPayments.Sum(x => x.Total));
            var depositsTotalAmount = deposits.Sum(x => x.Total);
            if (depositsTotalAmount != totalPaymentAmount)
                throw new VoucherAmountException($"amount for payment of deposits {string.Join(", ", deposits.Select(x => x.FriendlyId.DefaultIdPadding()))} ({depositsTotalAmount}) is different from amount in payment ({totalPaymentAmount})");

            deposits.ForEach(x => x.Close(closeTransactionId));
            //do not equalize existingPayments, since a dueDeposit is not a "real" Voucher 
        }

        private void CloseEqualizingPayments(List<Payment> existingPayments, Guid closeTransactionId)
        {
            var totalPaymentAmount = existingPayments.Sum(x => x.Total);
            if (totalPaymentAmount != 0)
                throw new VoucherAmountException($"amount for payments is not 0, cannot equalize payments");

            existingPayments.ForEach(x => x.Close(closeTransactionId));
            //do not equalize existingPayments, since a dueDeposit is not a "real" Voucher 
        }

        public void EqualizeInvoicesAndPayments(Payment newPayment,
            List<Payment> existingPayments, List<InvoiceBase> invoices,
            Guid closeTransactionId)
        {
            var totalPaymentAmount = 0 - ((newPayment?.Total ?? 0) + existingPayments.Sum(x => x.Total));
            var invoicesTotalAmount = invoices.Sum(x => x.Total);
            if (invoicesTotalAmount != totalPaymentAmount)
                throw new VoucherAmountException($"amount for payment of invoices {string.Join(", ", invoices.Select(x => x.FriendlyId.DefaultIdPadding()))} ({invoicesTotalAmount}) is different from amount in payment ({totalPaymentAmount})");

            CloseVouchers(newPayment, closeTransactionId, invoices, existingPayments);

            if (invoices.Count == 0) return;
            // if (invoices.OfType<Deposit>().Any()) throw new VoucherException("Cannot handle Deposit types");

            var invoice = invoices.Last(); //Last is implicitly the newly created invoice
            existingPayments.Where(x => x.SpecificInvoiceId != invoice.Id).ToList().ForEach(x =>
            {
                if (x.SpecificInvoiceId == null)
                {
                    x.SpecificInvoiceId = invoice.Id;
                    x.SpecificFriendlyInvoiceId = invoice.FriendlyId;
                }
                else
                {
                    throw new VoucherException(
                        $"Payment {x.FriendlyId.DefaultIdPadding()} cannot be assigned to invoice {invoice.FriendlyId.DefaultIdPadding()}. It is already assigned to {x.SpecificFriendlyInvoiceId.Value.DefaultIdPadding()}");
                }
            });
        }

        private void ClosePayment(Guid closeTransactionId, Payment payment, List<Voucher> originallyOpenVouchers)
        {
            var nowClosedInvoiceBases = originallyOpenVouchers.OfType<InvoiceBase>().Where(x => x.IsClosed).ToList();
            var ordersSpanCount = nowClosedInvoiceBases.Select(x => x.OrderId).Distinct().Count();

            payment.Close(closeTransactionId);

            if (ordersSpanCount == 1 && (nowClosedInvoiceBases.Sum(x => x.Total) + payment.Total == 0))
            {
                payment.OrderId = nowClosedInvoiceBases.First().OrderId;
                payment.FriendlyOrderId = nowClosedInvoiceBases.First().FriendlyOrderId;
            }
        }

        private void CloseVouchers(Payment newPayment, Guid closeTransactionId, List<InvoiceBase> invoices, List<Payment> existingPayments)
        {
            invoices.ForEach(x => x.Close(closeTransactionId));
            existingPayments.ForEach(x => ClosePayment(closeTransactionId, x, invoices.Cast<Voucher>().ToList()));

            if (newPayment != null)
            {
                ClosePayment(closeTransactionId, newPayment, invoices.Cast<Voucher>().ToList());
                if (invoices.Count == 1)
                {
                    newPayment.SetSpecificInvoice(invoices.First());
                }
            }
        }

        public async Task VerifyClosedTransactionAmountIsZero(Guid voucherCloseTransactionId)
        {
            var vouchers = await this._voucherRepository.FindVouchersFromClosedTransactionId(voucherCloseTransactionId);
            var sum = vouchers.Sum(x => x.Total);
            if (sum != 0)
            {
                throw new VoucherException("Sum of closed transaction should be 0, but is " + sum);
            }
        }
    }
}