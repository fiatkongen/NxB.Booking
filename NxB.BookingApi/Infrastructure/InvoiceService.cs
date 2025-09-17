using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Munk.AspNetCore;
using Newtonsoft.Json;
using NxB.BookingApi.Exceptions;
using NxB.BookingApi.Models;
using NxB.Domain.Common.Enums;
using NxB.Dto.AccountingApi;
using NxB.Dto.OrderingApi;

namespace NxB.BookingApi.Infrastructure
{
    public class InvoiceService : IInvoiceService
    {
        private readonly VoucherFactory _voucherFactory;
        private readonly IAccountRepository _accountRepository;
        private readonly IVoucherRepository _voucherRepository;
        private readonly ITranslatorService _translatorService;

        public InvoiceService(VoucherFactory voucherFactory, IAccountRepository accountRepository, IVoucherRepository voucherRepository, ITranslatorService translatorService)
        {
            _voucherFactory = voucherFactory;
            _accountRepository = accountRepository;
            _voucherRepository = voucherRepository;
            _translatorService = translatorService;
        }

        public async Task<Invoice> CreateInvoiceSpecific(Guid id, CreateSpecificInvoiceDto createSpecificInvoiceDto, OrderDto orderDto)
        {
            Validate(createSpecificInvoiceDto);
            var orderDtoForInvoicing = await RemoveInvoicedOrderLines(orderDto);

            var accountId = createSpecificInvoiceDto.AccountId;
            var account = _accountRepository.FindSingle(accountId);

            var invoice = _voucherFactory.CreateInvoice(id, account, orderDtoForInvoicing, createSpecificInvoiceDto.DueDate, createSpecificInvoiceDto.Language, createSpecificInvoiceDto.InvoiceTemplateId, createSpecificInvoiceDto.Note, createSpecificInvoiceDto.VoucherDate);

            await FillVoucher(createSpecificInvoiceDto, orderDtoForInvoicing, invoice, orderDto);
            return invoice;
        }

        public async Task<Invoice> CreateInvoice(Guid id, CreateInvoiceDto createInvoiceDto, OrderDto orderDto)
        {
            Validate(createInvoiceDto);
            orderDto = await RemoveInvoicedOrderLines(orderDto);

            var createSpecificInvoiceDto = CreateSpecificInvoiceDto(createInvoiceDto, orderDto);

            return await CreateInvoiceSpecific(id, createSpecificInvoiceDto, orderDto);
        }

        private static CreateSpecificInvoiceDto CreateSpecificInvoiceDto(CreateInvoiceDto createInvoiceDto, OrderDto orderDto)
        {
            var createSpecificInvoiceDto = new CreateSpecificInvoiceDto
            {
                AccountId = createInvoiceDto.AccountId,
                OrderId = createInvoiceDto.OrderId,
                InvoiceTemplateId = createInvoiceDto.InvoiceTemplateId,
                DueDate = createInvoiceDto.DueDate,
                Language = createInvoiceDto.Language,
                VoucherDate = createInvoiceDto.VoucherDate,
                CreateInvoiceSubOrders = orderDto.SubOrders.Select(x => new CreateInvoiceSubOrderDto
                {
                    SubOrderId = x.Id,
                    InvoiceOrderLinesIds = x.OrderLines.Select(ol => ol.Id).ToList(),
                }).ToList(),
            };
            return createSpecificInvoiceDto;
        }

        public async Task<Deposit> CreateDeposit(Guid id, CreateDepositDto createDepositDto, OrderDto orderDto, long? friendlyId)
        {
            Validate(createDepositDto);
            orderDto = await RemoveInvoicedOrderLines(orderDto);

            var createSpecificDepositDto = CreateSpecificDepositDto(createDepositDto, orderDto);

            return await this.CreateDepositSpecific(id, createSpecificDepositDto, orderDto, friendlyId);
        }

        private static CreateSpecificDepositDto CreateSpecificDepositDto(CreateDepositDto createDepositDto, OrderDto orderDto)
        {
            var createSpecificInvoiceDto = new CreateSpecificDepositDto
            {
                AccountId = createDepositDto.AccountId,
                OrderId = createDepositDto.OrderId,
                InvoiceTemplateId = createDepositDto.InvoiceTemplateId,
                DueDate = createDepositDto.DueDate,
                DepositAmount = createDepositDto.DepositAmount,
                DepositPercent = createDepositDto.DepositPercent,
                Language = createDepositDto.Language,
                VoucherDate = createDepositDto.VoucherDate,
                CreateInvoiceSubOrders = orderDto.SubOrders.Select(x => new CreateInvoiceSubOrderDto
                {
                    SubOrderId = x.Id,
                    InvoiceOrderLinesIds = x.OrderLines.Select(ol => ol.Id).ToList(),
                }).ToList()
            };
            return createSpecificInvoiceDto;
        }

        public async Task<Deposit> CreateDepositSpecific(Guid id, CreateSpecificDepositDto createSpecificDepositDto, OrderDto orderDto, long? friendlyId)
        {
            Validate(createSpecificDepositDto);
            var orderDtoForDeposit = await RemoveInvoicedOrderLines(orderDto);

            var accountId = createSpecificDepositDto.AccountId;
            var account = _accountRepository.FindSingle(accountId);

            var deposit = _voucherFactory.CreateDeposit(id, account, orderDtoForDeposit, createSpecificDepositDto.DueDate, createSpecificDepositDto.DepositPercent, createSpecificDepositDto.DepositAmount, createSpecificDepositDto.Language, createSpecificDepositDto.InvoiceTemplateId, createSpecificDepositDto.Note, createSpecificDepositDto.VoucherDate, friendlyId);
            await FillVoucher(createSpecificDepositDto, orderDtoForDeposit, deposit, orderDto);

            return deposit;
        }

        private static void Validate(CreateInvoiceDto createInvoiceDto)
        {
            if (createInvoiceDto.DueDate == DateTime.MinValue)
                throw new ArgumentException("CreateInvoiceDto.DueDate must be given");
        }

        public async Task<CreditNote> Credit(Guid invoiceToCreditId, Guid newCreditedInvoiceId, DateTime voucherDate)
        {
            var invoiceToCredit = _voucherRepository.FindSingleInvoiceBase<Invoice>(invoiceToCreditId);
            var creditedInvoice = _voucherFactory.CreateCreditNote(newCreditedInvoiceId, invoiceToCredit.AccountKey, invoiceToCredit, invoiceToCredit.OrderKey, invoiceToCredit.Language, null, voucherDate);
            var invoiceSubOrders = invoiceToCredit.InvoiceSubOrders;

            foreach (var invoiceSubOrder in invoiceSubOrders)
            {
                var newSubOrderId = Guid.NewGuid();
                creditedInvoice.CreateInvoiceSubOrder(newSubOrderId, invoiceSubOrder.SubOrderId, invoiceSubOrder.Index, invoiceSubOrder.Start, invoiceSubOrder.End, invoiceSubOrder.RentalUnitName);

                var invoiceLines = invoiceSubOrder.InvoiceLines;

                foreach (var invoiceLine in invoiceLines)
                {
                    var newInvoiceLineId = Guid.NewGuid();
                    if (invoiceLine is InvoiceTextLine)
                    {
                        creditedInvoice.CreateInvoiceTextLine(newInvoiceLineId, newSubOrderId, invoiceLine.Index, 0 - invoiceLine.Number, invoiceLine.Text, invoiceLine.PricePcs);
                    }
                    else if (invoiceLine is InvoiceOrderLine invoiceOrderLine)
                    {
                        creditedInvoice.CreateInvoiceOrderLine(newInvoiceLineId, newSubOrderId, invoiceOrderLine.OrderLineId, invoiceOrderLine.Index, 0 - invoiceOrderLine.Number, invoiceOrderLine.Text, invoiceOrderLine.Start, invoiceOrderLine.End, invoiceOrderLine.PriceProfileId, invoiceOrderLine.PriceProfileName, invoiceOrderLine.Tax, invoiceOrderLine.TaxPercent, invoiceOrderLine.PricePcs, invoiceOrderLine.IsDiscount);
                    }
                }
            }
            creditedInvoice.Total = 0 - invoiceToCredit.Total;
            creditedInvoice.SubTotal = 0 - invoiceToCredit.SubTotal;

            creditedInvoice.DocumentId = Guid.NewGuid();
            creditedInvoice.InvoiceId = invoiceToCredit.Id;
            creditedInvoice.FriendlyInvoiceId = invoiceToCredit.FriendlyId;
            creditedInvoice.TotalDifferenceText = invoiceToCredit.TotalDifferenceText;

            if (invoiceToCredit.IsOpen)
            {
                var voucherTransactionTotalId = Guid.NewGuid();
                invoiceToCredit.Close(voucherTransactionTotalId);
                creditedInvoice.Close(voucherTransactionTotalId);
            }
            else if (invoiceToCredit.IsClosed)
            {
                //creditNote should remain open
            }

            return creditedInvoice;
        }

        private async Task FillVoucher(ICreateInvoiceSubOrder createSpecificInvoiceDto, OrderDto orderDto, Invoice newlyCreatedInvoice, OrderDto originalOrderDto)
        {
            if (orderDto.SubOrders.Count == 0) throw new VoucherException("Kan ikke oprette faktura. Alle ordrelinier er allerede faktureret.");
            ValidateOrder(orderDto);
            var cleanedUpLanguages = _translatorService.CleanUpLanguages(createSpecificInvoiceDto.Language.Split(','));
            var invoiceSubOrderDtos = createSpecificInvoiceDto.CreateInvoiceSubOrders;
            foreach (var invoiceSubOrderDto in invoiceSubOrderDtos)
            {
                var subOrderDto = orderDto.SubOrders.Single(x => x.Id == invoiceSubOrderDto.SubOrderId);
                var originalSubOrderDto = originalOrderDto.SubOrders.Single(x => x.Id == invoiceSubOrderDto.SubOrderId);
                var newInvoiceSubOrderId = Guid.NewGuid();
                newlyCreatedInvoice.CreateInvoiceSubOrder(newInvoiceSubOrderId, subOrderDto.Id, subOrderDto.Index, subOrderDto.Start, subOrderDto.End, originalSubOrderDto.GetAllocationUnitsCombinedText());

                var invoiceOrderLinesIds = invoiceSubOrderDto.InvoiceOrderLinesIds;
                bool invoiceSubOrderContainsValidOrderLine = false;

                foreach (var invoiceOrderLineId in invoiceOrderLinesIds)
                {
                    var orderLineDto = subOrderDto.OrderLines.Single(x => x.Id == invoiceOrderLineId);
                    if (!IsOrderLineValidForInvoicing(orderLineDto))
                    {
                        continue;
                    }
                    var newInvoiceOrderLineId = Guid.NewGuid();

                    DateTime? start = null;
                    DateTime? end = null;

                    if (orderLineDto is TimeBasedOrderLineDto timeBasedOrderLineDto)
                    {
                        start = timeBasedOrderLineDto.Start;
                        end = timeBasedOrderLineDto.End;
                    }

                    var invoiceText = orderLineDto.Text;
                    //if (orderLineDto is AllocationOrderLineDto allocationOrderLineDto)
                    //{
                    //    invoiceText = await _translatorService.TryTranslateRentalUnit(allocationOrderLineDto.ResourceId, cleanedUpLanguages);
                    //}
                    //else if (orderLineDto is GuestOrderLineDto guestOrderLineDto)
                    //{
                    //    invoiceText = await _translatorService.TryTranslateGuestCategory(guestOrderLineDto.ResourceId, cleanedUpLanguages);
                    //}
                    //else if (orderLineDto is ArticleOrderLineDto articleOrderLineDto)
                    //{
                    //    invoiceText = await _translatorService.TryTranslateArticle(articleOrderLineDto.ResourceId, cleanedUpLanguages);
                    //}

                    invoiceSubOrderContainsValidOrderLine = true;
                    newlyCreatedInvoice.CreateInvoiceOrderLine(newInvoiceOrderLineId, newInvoiceSubOrderId, invoiceOrderLineId, orderLineDto.Index, orderLineDto.Number, invoiceText, start, end, orderLineDto.PriceProfileId, orderLineDto.PriceProfileName, orderLineDto.Tax, orderLineDto.TaxPercent, orderLineDto.PricePcs, orderLineDto is SubOrderDiscountLineDto);
                }

                if (!invoiceSubOrderContainsValidOrderLine)
                {
                    newlyCreatedInvoice.RemoveInvoiceSubOrder(newInvoiceSubOrderId);
                    continue;
                }

                //Build invoice text lines for the subOrderLines
                var subOrderDiscountDtos = subOrderDto.SubOrderDiscounts.OrderBy(x => x.Index);

                foreach (var subOrderDiscountDto in subOrderDiscountDtos)
                {
                    var subOrderDiscountLineDtos = subOrderDto.SubOrderDiscountLines.Where(x => x.SubOrderDiscountId == subOrderDiscountDto.Id && invoiceOrderLinesIds.Contains(x.Id)).ToList();
                    if (subOrderDiscountLineDtos.Count == 0) continue;

                    var subOrderDiscountTotal = subOrderDiscountLineDtos.Sum(x => x.PricePcs);
                    var newInvoiceOrderLineId = Guid.NewGuid();
                    newlyCreatedInvoice.CreateInvoiceTextLine(newInvoiceOrderLineId, newInvoiceSubOrderId, subOrderDiscountDto.Index, 1, subOrderDiscountDto.Text, subOrderDiscountTotal);
                }
            }
            newlyCreatedInvoice.CalculateTotals();
            newlyCreatedInvoice.DocumentId = Guid.NewGuid();
        }

        public async Task<OrderDto> RemoveInvoicedOrderLines(OrderDto orderDto)
        {
            var newOrderDto = orderDto.CloneJson();
            var invoicedOrderLineIds = newOrderDto.SubOrders.SelectMany(x => x.OrderLines).Select(x => x.Id).ToList();
            var notInvoicedOrderLineIds = await _voucherRepository.RemoveInvoicedOrderLineIds(invoicedOrderLineIds);

            newOrderDto.SubOrders.ForEach(x => x.AllocationOrderLines = x.AllocationOrderLines.Where(ol => !ol.IsEqualized && notInvoicedOrderLineIds.Any(ni => ni == ol.Id)).ToList());
            newOrderDto.SubOrders.ForEach(x => x.ArticleOrderLines = x.ArticleOrderLines.Where(ol => !ol.IsEqualized && notInvoicedOrderLineIds.Any(ni => ni == ol.Id)).ToList());
            newOrderDto.SubOrders.ForEach(x => x.GuestOrderLines = x.GuestOrderLines.Where(ol => !ol.IsEqualized && notInvoicedOrderLineIds.Any(ni => ni == ol.Id)).ToList());
            newOrderDto.SubOrders.ForEach(x => x.DiscountOrderLines = x.DiscountOrderLines.Where(ol => !ol.IsEqualized && notInvoicedOrderLineIds.Any(ni => ni == ol.Id)).ToList());
            newOrderDto.SubOrders.ForEach(x => x.SubOrderDiscountLines = x.SubOrderDiscountLines.Where(ol => !ol.IsEqualized && notInvoicedOrderLineIds.Any(ni => ni == ol.Id)).ToList());

            newOrderDto.SubOrders = newOrderDto.SubOrders.Where(x => x.OrderLines.Count > 0).ToList();
            return newOrderDto;
        }

        public async Task<Invoice> DeleteLegacyInvoice(Guid id)
        {
            var invoiceToDelete = _voucherRepository.FindSingleInvoiceBase<Invoice>(id);
            if (invoiceToDelete.FriendlyId > 0) throw new VoucherException("Kan ikke slette faktura. Faktura skal have et negativt(gammelt) fakturanumer for at den kan slettes");

            if (invoiceToDelete.VoucherCloseTransactionId.HasValue)
            {
                var vouchers = await _voucherRepository.FindVouchersFromClosedTransactionId(invoiceToDelete.VoucherCloseTransactionId.Value);
                var payments = vouchers.OfType<Payment>().ToList();

                //hack, open all payments
                payments.ForEach(x =>
                {
                    x.ForceOpenHack();
                    _voucherRepository.Update(x);
                });
            }
            _voucherRepository.DeleteLegacyInvoice(invoiceToDelete);

            return invoiceToDelete;
        }

        private void ValidateOrder(OrderDto orderDto)
        {

        }

        private bool IsOrderLineValidForInvoicing(OrderLineDto orderLineDto)
        {
            return !orderLineDto.IsEqualized;
        }

        public async Task<Payment> CreatePayment(OrderDto orderDto, decimal amount, PaymentType paymentType, string language,
            DateTime? paymentDate, List<Guid> invoiceIds, List<Guid> existingPaymentIds, IEqualizeService equalizeService, DateTime? overrideCreateDate, AppDbContext appDbContext, Guid? overridePaymentId, Guid? specificInvoiceId = null, long? specificFriendlyInvoiceId = null)
        {
            var account = _accountRepository.FindSingle(orderDto.AccountId);
            var payment = _voucherFactory.CreatePayment(overridePaymentId ?? Guid.NewGuid(), account, amount, paymentType, orderDto, language, paymentDate, null, specificInvoiceId, specificFriendlyInvoiceId);

            if (overrideCreateDate != null)
            {
                payment.CreateDate = overrideCreateDate.Value;
                payment.VoucherDate = overrideCreateDate.Value;
            }

            _voucherRepository.Add(payment);
            await appDbContext.SaveChangesAsync();
            
            Guid closeTransactionId = Guid.NewGuid();
            await equalizeService.EqualizeVouchersAndPayments(payment, existingPaymentIds ?? new List<Guid>(), invoiceIds ?? new List<Guid>(), this, closeTransactionId);
            await appDbContext.SaveChangesAsync();
            await equalizeService.VerifyClosedTransactionAmountIsZero(closeTransactionId);
            
            return payment;
        }

        public async Task<Payment> CreateOnlinePayment(OrderDto orderDto, decimal amount, DueVoucher dueVoucher, IEqualizeService equalizeService, AppDbContext appDbContext)
        {
            var account = _accountRepository.FindSingle(orderDto.AccountId);
            var payment = _voucherFactory.CreatePayment(Guid.NewGuid(), account, amount, PaymentType.Online, orderDto, dueVoucher.Language, null, null, null, null);

            _voucherRepository.Add(payment);
            await appDbContext.SaveChangesAsync();

            Guid closeTransactionId = Guid.NewGuid();
            bool equalized = await equalizeService.TryEqualizeInvoiceWithSpecificPayment(payment, dueVoucher.Id, this, closeTransactionId);
            await appDbContext.SaveChangesAsync();

            if (equalized)
            {
                await equalizeService.VerifyClosedTransactionAmountIsZero(closeTransactionId);
            }

            if (dueVoucher.IsClosed) { return payment; } //take into account it might have been paid in the meantime

            return payment;
        }

        public async Task EqualizeVoucher(Guid orderId, List<Guid> invoiceIds, IEqualizeService equalizeService, AppDbContext appDbContext)
        {
            var existingPayments = await _voucherRepository.FindPaymentsOrCreditNotesFromOrder(orderId, false);
            var existingPaymentIds = existingPayments.Select(x => x.Id).ToList();

            Guid closeTransactionId = Guid.NewGuid();
            await equalizeService.EqualizeVouchersAndPayments(null, existingPaymentIds, invoiceIds, this, closeTransactionId);
            await appDbContext.SaveChangesAsync();
            await equalizeService.VerifyClosedTransactionAmountIsZero(closeTransactionId);
        }
    }
}