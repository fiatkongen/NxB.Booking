using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;
using AutoMapper;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Munk.AspNetCore;
using Munk.Azure.Storage;
using Munk.Utils.Object;
using NxB.BookingApi.Exceptions;
using NxB.BookingApi.Infrastructure;
using NxB.BookingApi.Models;
using NxB.Domain.Common.Enums;
using NxB.Domain.Common.Interfaces;
using NxB.Dto.AccountingApi;
using NxB.Dto.Clients;
using NxB.Dto.Exceptions;
using NxB.Dto.OrderingApi;
using NxB.MemCacheActor.Interfaces;
using NxB.Remoting.Interfaces.SignalrApi;

namespace NxB.BookingApi.Controllers.Accounting
{
    [Produces("application/json")]
    [Route("voucher")]
    [Authorize]
    [ApiValidationFilter]
    public class VoucherController : BaseController
    {
        private readonly IVoucherRepository _voucherRepository;
        private readonly IInvoiceService _invoiceService;
        private readonly AppDbContext _appDbContext;
        private readonly VoucherMapper _voucherMapper;
        private readonly IEqualizeService _equalizeService;
        private readonly IDocumentClient _documentClient;
        private readonly IOrderRepository _orderRepository;
        private readonly IAccountRepository _accountRepository;
        private readonly VoucherFactory _voucherFactory;
        private readonly IPaymentService _paymentService;
        private readonly IMapper _mapper;
        private readonly IFriendlyAccountingIdProvider _friendlyIdProvider;
        private readonly IGroupedBroadcasterClient _groupedBroadcasterClient;
        private readonly TelemetryClient _telemetryClient;
        private readonly IMemCacheActor _memCacheActor;
        private readonly IVoucherClient _voucherClient;

        public VoucherController(
            IVoucherRepository voucherRepository,
            AppDbContext appDbContext,
            IInvoiceService invoiceService,
            VoucherMapper voucherMapper,
            IEqualizeService equalizeService,
            IDocumentClient documentClient,
            IOrderRepository orderRepository,
            IAccountRepository accountRepository,
            VoucherFactory voucherFactory,
            IPaymentService paymentService,
            IMapper mapper,
            IFriendlyAccountingIdProvider friendlyIdProvider,
            IGroupedBroadcasterClient groupedBroadcasterClient, 
            TelemetryClient telemetryClient, IMemCacheActor memCacheActor, IVoucherClient voucherClient)
        {
            _voucherRepository = voucherRepository;
            _appDbContext = appDbContext;
            _invoiceService = invoiceService;
            _voucherMapper = voucherMapper;
            _equalizeService = equalizeService;
            _documentClient = documentClient;
            _orderRepository = orderRepository;
            _accountRepository = accountRepository;
            _voucherFactory = voucherFactory;
            _paymentService = paymentService;
            _mapper = mapper;
            _friendlyIdProvider = friendlyIdProvider;
            _groupedBroadcasterClient = groupedBroadcasterClient;
            _telemetryClient = telemetryClient;
            _memCacheActor = memCacheActor;
            _voucherClient = voucherClient;
        }

        [HttpPost]
        [Route("invoice")]
        public async Task<ObjectResult> CreateInvoice([FromBody] CreateInvoiceDto createInvoiceDto)
        {
            using var transactionScope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.Snapshot }, TransactionScopeAsyncFlowOption.Enabled);
            var orderDto = await _orderClient.FindOrder(createInvoiceDto.OrderId);
            var invoice = await this._invoiceService.CreateInvoice(Guid.NewGuid(), createInvoiceDto, orderDto);
            _voucherRepository.Add(invoice);
            await _appDbContext.SaveChangesAsync();

            Payment payment = null;
            if (createInvoiceDto.PaymentType != PaymentType.None &&
                createInvoiceDto.PaymentType != PaymentType.Equalize &&
                createInvoiceDto.PaymentAmount > 0)
            {
                payment = await CreateDefaultPayment(createInvoiceDto.PaymentType, createInvoiceDto.PaymentAmount, orderDto, invoice, null, createInvoiceDto.OverridePaymentId);
            }

            Guid closeTransactionId = Guid.NewGuid();
            await _equalizeService.EqualizeVouchersAndPayments(payment, new List<Guid>(), new List<Guid> { invoice.Id }, _invoiceService, closeTransactionId);
            await _appDbContext.SaveChangesAsync();
            await _equalizeService.VerifyClosedTransactionAmountIsZero(closeTransactionId);

            var readVoucherDto = this._voucherMapper.Map<ReadVoucherDto, Invoice>(invoice, new List<CreditNote>());
            var paymentDto = _voucherMapper.Map<PaymentDto, Payment>(payment, new List<CreditNote>());

            transactionScope.Complete();

            await _voucherClient.BroadcastDueInvoicesCount();

            if (!createInvoiceDto.SkipDocumentCreate || createInvoiceDto.OverrideDocumentText != null)
            {
                try
                {
                    await _documentClient.GenerateVoucherPdf(readVoucherDto, paymentDto, VoucherTemplateType.Invoice,
                        createInvoiceDto.Language, createInvoiceDto.InvoiceTemplateId, createInvoiceDto.OverrideDocumentText);
                }
                catch (Exception exception)
                {
                    throw new PdfGenerateException(
                        "Fakturaen er genereret, men ikke dokumentet (Pdf). Denne vil dog blive genereret næste gang du forsøger at åbne bilaget.",
                        exception);
                }
            }

            var invoiceDto = this._voucherMapper.Map<InvoiceDto, Invoice>(invoice, new List<CreditNote>());
            return new CreatedResult(new Uri("?id=" + invoice.Id, UriKind.Relative), invoiceDto);
        }

        [HttpPost]
        [Route("invoice/specific")]
        public async Task<ObjectResult> CreateInvoiceSpecific([FromBody] CreateSpecificInvoiceDto createSpecificInvoiceDto)
        {
            using var transactionScope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.Snapshot }, TransactionScopeAsyncFlowOption.Enabled);

            var orderDto = await _orderClient.FindOrder(createSpecificInvoiceDto.OrderId);
            var invoice = await this._invoiceService.CreateInvoiceSpecific(Guid.NewGuid(), createSpecificInvoiceDto, orderDto);
            _voucherRepository.Add(invoice);
            await _appDbContext.SaveChangesAsync();

            Guid closeTransactionId = Guid.NewGuid();
            Payment payment = null;

            if (createSpecificInvoiceDto.PaymentType != PaymentType.None && createSpecificInvoiceDto.PaymentType != PaymentType.Equalize && createSpecificInvoiceDto.PaymentAmount != 0)
            {
                payment = await CreateDefaultPayment(createSpecificInvoiceDto.PaymentType, createSpecificInvoiceDto.PaymentAmount, orderDto, invoice, null, createSpecificInvoiceDto.OverridePaymentId);
            }

            var invoiceIds = new List<Guid>();
            invoiceIds.AddRange(createSpecificInvoiceDto.InvoiceIds);
            invoiceIds.Add(invoice.Id);

            await _equalizeService.EqualizeVouchersAndPayments(payment, createSpecificInvoiceDto.ExistingPaymentIds, invoiceIds, _invoiceService, closeTransactionId);
            await _appDbContext.SaveChangesAsync();
            await _equalizeService.VerifyClosedTransactionAmountIsZero(closeTransactionId);

            var readVoucherDto = this._voucherMapper.Map<ReadVoucherDto, Invoice>(invoice, new List<CreditNote>());
            var invoiceDto = this._voucherMapper.Map<InvoiceDto, Invoice>(invoice, new List<CreditNote>());
            var paymentDto = _voucherMapper.Map<PaymentDto, Payment>(payment, new List<CreditNote>());

            transactionScope.Complete();

            await _voucherClient.BroadcastDueInvoicesCount();

            if (!createSpecificInvoiceDto.SkipDocumentCreate || createSpecificInvoiceDto.OverrideDocumentText != null)
            {
                try
                {
                    await _documentClient.GenerateVoucherPdf(readVoucherDto, paymentDto, VoucherTemplateType.Invoice,
                        createSpecificInvoiceDto.Language, createSpecificInvoiceDto.InvoiceTemplateId, createSpecificInvoiceDto.OverrideDocumentText);
                }
                catch (Exception exception)
                {
                    throw new PdfGenerateException(
                        "Fakturaen er genereret, men ikke dokumentet (Pdf). Denne vil dog blive genereret næste gang du forsøger at åbne bilaget.",
                        exception);
                }
            }
            return new CreatedResult(new Uri("?id=" + invoice.Id, UriKind.Relative), invoiceDto);
        }

        [HttpPost]
        [Route("duedeposit")]
        public async Task<ObjectResult> CreateDueDeposit([FromBody] CreateDepositDto createDepositDto)
        {
            if (createDepositDto.PaymentType != PaymentType.None)
            {
                throw new VoucherException("Kan ikke oprette et depositum med en straks-indbetaling");
            }

            var friendlyId = await _memCacheActor.GenerateNextFriendlyDueDepositId(GetTenantId());

            using var transactionScope = new TransactionScope(TransactionScopeOption.Required,
                new TransactionOptions { IsolationLevel = IsolationLevel.Snapshot },
                TransactionScopeAsyncFlowOption.Enabled);
            var orderDto = await _orderClient.FindOrder(createDepositDto.OrderId);
//            var friendlyId = _friendlyIdProvider.GenerateNextFriendlyDueDepositId();
            var deposit = await this._invoiceService.CreateDeposit(Guid.NewGuid(), createDepositDto, orderDto, friendlyId);
            deposit.DocumentId = createDepositDto.SaveId ??  Guid.NewGuid();

            //do not add deposit to _voucherRepo, as to not create a "real" invoice. This avoids locking orderLines from being invoiced, and makes sure that the deposit is not included as an invoice
            //_voucherRepository.Add(deposit);
            var dueDeposit = this._voucherFactory.CreateDueDeposit(Guid.NewGuid(), deposit.FriendlyId, deposit.AccountKey, deposit.OrderKey, deposit.DueDate, deposit.DepositPercent, deposit.DepositAmount, deposit.SubTotal, deposit.Language, deposit.DocumentTemplateId, deposit.Note, deposit.VoucherDate);
            dueDeposit.DocumentId = deposit.DocumentId;
            _voucherRepository.Add(dueDeposit);

            _telemetryClient.TrackTrace($"CreateDueDeposit deposit.FriendlyId={deposit.FriendlyId}, voucherType={deposit.VoucherType}, tenantId={GetTenantId()}");

            try
            {
                await _appDbContext.SaveChangesAsync();
            }
            catch (SqlException ex)
            {
                _telemetryClient.TrackTrace("Error creating duedeposit");
                _telemetryClient.TrackException(ex);
                throw;
            }

            var dueDepositDto = await this.CreateDueDepositGeneric(dueDeposit);
            var readVoucherDto = this._voucherMapper.Map<ReadVoucherDto, Deposit>(deposit, new List<CreditNote>());
            readVoucherDto.VoucherType = dueDepositDto.VoucherType;

            try
            {
                await _documentClient.GenerateVoucherPdf(readVoucherDto, null, VoucherTemplateType.Deposit,
                    createDepositDto.Language, createDepositDto.InvoiceTemplateId,
                    createDepositDto.OverrideDocumentText);
                transactionScope.Complete();
            }
            catch (Exception exception)
            {
                _telemetryClient.TrackTrace("Error creating duedeposit2");
                throw new PdfGenerateException("Kunne ikke generere opkrævning.", exception);
            }
            await _voucherClient.BroadcastDueDepositsCount();

            return new CreatedResult(new Uri("?id=" + dueDeposit.Id, UriKind.Relative), dueDepositDto);
        }

        [HttpPost]
        [Route("duedeposit/readvoucherdto")]
        public async Task<ReadVoucherDto> CreateReadVoucherDto([FromBody] CreateReadVoucherDto createReadVoucherDto)
        {
            var orderDto = createReadVoucherDto.OrderDto;
            var friendlyId = createReadVoucherDto.FriendlyVoucherId;
            var createDepositDto = createReadVoucherDto.CreateDepositDto;
            var deposit = await this._invoiceService.CreateDeposit(Guid.NewGuid(), createDepositDto, orderDto, friendlyId);
            deposit.DocumentId = createDepositDto.SaveId ?? Guid.NewGuid();

            var dueDeposit = this._voucherFactory.CreateDueDeposit(Guid.NewGuid(), deposit.FriendlyId, deposit.AccountKey, deposit.OrderKey, deposit.DueDate, deposit.DepositPercent, deposit.DepositAmount, deposit.SubTotal, deposit.Language, deposit.DocumentTemplateId, deposit.Note, deposit.VoucherDate);
            dueDeposit.DocumentId = deposit.DocumentId;

            var dueDepositDto = await this.CreateDueDepositGeneric(dueDeposit);
            var readVoucherDto = this._voucherMapper.Map<ReadVoucherDto, Deposit>(deposit, new List<CreditNote>());
            readVoucherDto.VoucherType = dueDepositDto.VoucherType;

            return readVoucherDto;
        }

        [HttpPost]
        [Route("duedeposit/specific")]
        public async Task<ObjectResult> CreateDueDepositSpecific([FromBody] CreateSpecificDepositDto createSpecificDepositDto)
        {
            if (createSpecificDepositDto.PaymentType != PaymentType.None)
            {
                throw new VoucherException("Kan ikke oprette et depositum med en straks-indbetaling");
            }

            var friendlyId = await _memCacheActor.GenerateNextFriendlyDueDepositId(GetTenantId());

            using var transactionScope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.Snapshot }, TransactionScopeAsyncFlowOption.Enabled);
            var orderDto = await _orderClient.FindOrder(createSpecificDepositDto.OrderId);
            var deposit = await this._invoiceService.CreateDepositSpecific(Guid.NewGuid(), createSpecificDepositDto, orderDto, friendlyId);
            deposit.DocumentId = Guid.NewGuid();

            //do not add deposit to _voucherRepo, as to not create a "real" invoice. This avoids locking orderLines from being invoiced, and makes sure that the deposit is not included as an invoice
            //_voucherRepository.Add(deposit);
            //instead add a DueDeposit
            var dueDeposit = this._voucherFactory.CreateDueDeposit(Guid.NewGuid(), deposit.FriendlyId, deposit.AccountKey, deposit.OrderKey, deposit.DueDate, deposit.DepositPercent, deposit.DepositAmount, deposit.SubTotal, deposit.Language, deposit.DocumentTemplateId, deposit.Note, deposit.VoucherDate);
            dueDeposit.DocumentId = deposit.DocumentId;

            _voucherRepository.Add(dueDeposit);

            await _appDbContext.SaveChangesAsync();

            var dueDepositDto = await this.CreateDueDepositGeneric(dueDeposit);
            var readVoucherDto = this._voucherMapper.Map<ReadVoucherDto, Deposit>(deposit, new List<CreditNote>());
            readVoucherDto.VoucherType = dueDepositDto.VoucherType;

            try
            {
                await _documentClient.GenerateVoucherPdf(readVoucherDto, null, VoucherTemplateType.Deposit,
                    createSpecificDepositDto.Language, createSpecificDepositDto.InvoiceTemplateId,
                    createSpecificDepositDto.OverrideDocumentText);
                transactionScope.Complete();
            }
            catch (Exception exception)
            {
                throw new PdfGenerateException("Kunne ikke generere opkrævning.", exception);
            }

            await _voucherClient.BroadcastDueDepositsCount();
            return new CreatedResult(new Uri("?id=" + dueDeposit.Id, UriKind.Relative), dueDepositDto);
        }

        public async Task<DepositDto> CreateDepositGeneric(Deposit deposit)
        {
            var depositDto = this._voucherMapper.Map<DepositDto, Deposit>(deposit, new List<CreditNote>());
            return depositDto;
        }

        public async Task<DueDepositDto> CreateDueDepositGeneric(DueDeposit dueDeposit)
        {
            var depositDto = this._voucherMapper.Map<DueDepositDto, DueDeposit>(dueDeposit, new List<CreditNote>());
            return depositDto;
        }

        private async Task<Payment> CreateDefaultPayment(PaymentType defaultPayment, decimal? paymentAmount, OrderDto orderDto, Invoice invoice, DateTime? paymentDate, Guid? overridePaymentId)
        {
            if (defaultPayment == PaymentType.None) return null;
            var account = _accountRepository.FindSingle(orderDto.AccountId);
            var payment = _voucherFactory.CreatePayment(overridePaymentId ?? Guid.NewGuid(), account, paymentAmount ?? invoice.Total, defaultPayment, orderDto, invoice.Language, paymentDate, null, invoice.Id, invoice.FriendlyId);
            _voucherRepository.Add(payment);
            await _appDbContext.SaveChangesAsync();

            return payment;
        }

        [HttpPost]
        [Route("payment")]
        public async Task<ObjectResult> CreatePayment([FromBody] CreatePaymentDto createPaymentDto)
        {
            using var transactionScope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.Snapshot }, TransactionScopeAsyncFlowOption.Enabled);
            var orderDto = await _orderClient.FindOrder(createPaymentDto.OrderId);

            var payment = await _invoiceService.CreatePayment(orderDto, createPaymentDto.PaymentAmount, createPaymentDto.PaymentType,
                createPaymentDto.Language, createPaymentDto.PaymentDate, createPaymentDto.InvoiceIds,
                createPaymentDto.ExistingPaymentIds, _equalizeService, createPaymentDto.OverrideCreateDate,
                _appDbContext, createPaymentDto.OverridePaymentId);

            var paymentDto = _voucherMapper.Map<PaymentDto, Payment>(payment, null);
            
            transactionScope.Complete();
            await _voucherClient.BroadcastDueVouchersCount();

            if (!createPaymentDto.SkipDocumentCreate)
            {
                await GeneratePaymentVoucherPdf(payment);
            }

            return new CreatedResult(new Uri("?id=" + payment.Id, UriKind.Relative), paymentDto);
        }

        [HttpPost]
        [Route("payment/specific")]
        public async Task<ObjectResult> CreateSpecificPayment([FromBody] CreateSpecificPaymentDto dto)
        {
            using var transactionScope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.Snapshot }, TransactionScopeAsyncFlowOption.Enabled);
            var orderDto = await _orderClient.FindOrder(dto.OrderId);
            var account = _accountRepository.FindSingle(orderDto.AccountId);

            var specificPayment = _voucherFactory.CreateSpecificPayment(Guid.NewGuid(), account, dto.PaymentAmount, dto.PaymentType, orderDto, dto.SpecificInvoiceId, dto.SpecificFriendlyInvoiceId, dto.Language, dto.PaymentDate, null);

            _voucherRepository.Add(specificPayment);
            await _appDbContext.SaveChangesAsync();

            Guid closeTransactionId = Guid.NewGuid();
            bool equalized = await _equalizeService.TryEqualizeInvoiceWithSpecificPayment(specificPayment, dto.SpecificInvoiceId, _invoiceService, closeTransactionId);
            await _appDbContext.SaveChangesAsync();

            if (equalized)
            {
                await _equalizeService.VerifyClosedTransactionAmountIsZero(closeTransactionId);
            }

            var paymentDto = _voucherMapper.Map<PaymentDto, Payment>(specificPayment, null);

            transactionScope.Complete();

            await _voucherClient.BroadcastDueVouchersCount();

            if (!dto.SkipDocumentCreate)
            {
                await GeneratePaymentVoucherPdf(specificPayment);
            }

            return new CreatedResult(new Uri("?id=" + specificPayment.Id, UriKind.Relative), paymentDto);
        }

        private async Task GeneratePaymentVoucherPdf(Payment payment)
        {
            try
            {
                var readVoucherDto = this._voucherMapper.Map<ReadVoucherDto, Payment>(payment, new List<CreditNote>());
                await _documentClient.GenerateVoucherPdf(readVoucherDto, null, VoucherTemplateType.Payment, payment.Language, null, null);
            }
            catch (Exception exception)
            {
                throw new PdfGenerateException("Indbetalingen er genereret, men ikke dokumentet (Pdf). Denne vil dog blive genereret næste gang du forsøger at åbne bilaget.", exception);
            }
        }

        [HttpPut]
        [Route("payments/equalize")]
        public async Task<IActionResult> EqualizePayments([FromBody] EqualizePaymentsDto createPaymentDto)
        {
            using var transactionScope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.Snapshot }, TransactionScopeAsyncFlowOption.Enabled);

            Guid closeTransactionId = Guid.NewGuid();
            await _equalizeService.EqualizeVouchersAndPayments(null, createPaymentDto.ExistingPaymentIds, createPaymentDto.InvoiceIds, _invoiceService, closeTransactionId);
            await _appDbContext.SaveChangesAsync();
            await _equalizeService.VerifyClosedTransactionAmountIsZero(closeTransactionId);
            transactionScope.Complete();

            await _voucherClient.BroadcastDueVouchersCount();

            return new OkResult();
        }

        [HttpGet]
        [Route("")]
        public ObjectResult FindSingleVoucher(Guid voucherId)
        {
            var voucher = _voucherRepository.FindSingleVoucher<Voucher>(voucherId);
            if (voucher == null) { return new ObjectResult(null); }

            var voucherDto = _voucherMapper.Map<ReadVoucherDto, Voucher>(voucher, new List<CreditNote>());
            return new OkObjectResult(voucherDto);
        }

        [HttpGet]
        [Route("friendlyid")]
        public ObjectResult FindSingleVoucherFromFriendlyId(long friendlyVoucherId, VoucherType voucherType)
        {
            var voucher = _voucherRepository.FindSingleOrDefaultVoucherFromFriendlyId<Voucher>(friendlyVoucherId, voucherType);
            if (voucher == null) { return new ObjectResult(null); }
            var voucherDto = _voucherMapper.Map<ReadVoucherDto, Voucher>(voucher, new List<CreditNote>());
            return new OkObjectResult(voucherDto);
        }

        [HttpGet]
        [Route("invoice")]
        public ObjectResult FindInvoice([Required(AllowEmptyStrings = false)] string invoiceId)
        {
            bool isFriendly = int.TryParse(invoiceId, out var friendlyId);
            Invoice invoice;

            if (isFriendly)
            {
                invoice = _voucherRepository.FindSingleInvoiceFromFriendlyId<Invoice>(friendlyId);
            }
            else
            {
                invoice = _voucherRepository.FindSingleInvoiceBase<Invoice>(Guid.Parse(invoiceId));
            }

            var creditNotes = new List<CreditNote>();
            var creditNoteFromInvoiceId = _voucherRepository.FindSingleOrDefaultCreditNoteFromInvoiceId(invoice.Id);
            if (creditNoteFromInvoiceId != null) creditNotes.Add(creditNoteFromInvoiceId);
            var invoiceDto = _voucherMapper.Map<InvoiceDto, Invoice>(invoice, creditNotes);
            PopulateIsCreditedOnInvoiceDto(invoiceDto);
            return new OkObjectResult(invoiceDto);
        }

        [HttpGet]
        [Route("duedeposit")]
        public ObjectResult FindDueDeposit([NoEmpty] Guid depositId)
        {
            var dueDeposit = _voucherRepository.FindSingleVoucher<DueDeposit>(depositId);
            var dueDepositDto = _voucherMapper.Map<DueDepositDto, DueDeposit>(dueDeposit, new List<CreditNote>());

            return new OkObjectResult(dueDepositDto);
        }

        [HttpGet]
        [Route("payment")]
        public ObjectResult FindPayment([NoEmpty] Guid id)
        {
            var payment = _voucherRepository.FindSinglePayment(id);
            var paymentDto = _voucherMapper.Map<PaymentDto, Payment>(payment, null);
            return new OkObjectResult(paymentDto);
        }

        [HttpGet]
        [Route("list/all")]
        public async Task<ObjectResult> FindAllVouchers(DateTime start, DateTime end, bool? isClosed = null)
        {
            var vouchers = await _voucherRepository.FindAll<Voucher>(new DateInterval(start, end, true), isClosed);
            var voucherDtos = vouchers.Select(x => _voucherMapper.Map<ReadVoucherDto, Voucher>(x, null)).ToList();
            return new OkObjectResult(voucherDtos);
        }

        [HttpGet]
        [Route("creditnote/list/all")]
        public async Task<ObjectResult> FindAllCreditNotes(DateTime start, DateTime end, bool? isClosed = null)
        {
            var vouchers = await _voucherRepository.FindAll<CreditNote>(new DateInterval(start, end, true), isClosed);
            var voucherDtos = vouchers.Select(x => _voucherMapper.Map<CreditNoteDto, CreditNote>(x, null)).ToList();
            return new OkObjectResult(voucherDtos);
        }

        [HttpGet]
        [Route("readvoucher/list/all")]
        public async Task<ObjectResult> FindAllReadVouchers(DateTime start, DateTime end, bool? isClosed = null)
        {
            var vouchers = await _voucherRepository.FindAll<Voucher>(new DateInterval(start, end, true), isClosed);
            var creditNotes = vouchers.OfType<CreditNote>().ToList();
            var voucherDtos = vouchers.Select(x => _voucherMapper.Map<ReadVoucherDto, Voucher>(x, creditNotes)).ToList();
            return new OkObjectResult(voucherDtos);
        }

        [HttpGet]
        [Route("readvoucher/documentid")]
        public async Task<ObjectResult> FinReadVoucherFromDocumentId(Guid documentId)
        {
            var voucher = await _voucherRepository.FindSingleFromDocumentId<Voucher>(documentId);
            var voucherDto = _mapper.Map<ReadVoucherDto>(voucher);
            return new OkObjectResult(voucherDto);
        }

        [HttpGet]
        [Route("list/all/account")]
        public async Task<ObjectResult> FindAllVouchersFromAccountId(Guid accountId, bool? isClosed = null)
        {
            var vouchers = await _voucherRepository.FindFromAccountId<Voucher>(accountId, isClosed);
            var creditNotes = vouchers.OfType<CreditNote>().ToList();
            var voucherDtos = vouchers.Select(x => _voucherMapper.Map<VoucherDto, Voucher>(x, creditNotes)).ToList();
            return new OkObjectResult(voucherDtos);
        }

        [HttpGet]
        [Route("readvoucher/list/all/account")]
        public async Task<ObjectResult> FindAllReadVouchersFromAccountId(Guid accountId, bool? isClosed = null)
        {
            var vouchers = await _voucherRepository.FindFromAccountId<Voucher>(accountId, isClosed);
            var creditNotes = vouchers.OfType<CreditNote>().ToList();
            var voucherDtos = vouchers.Select(x => _voucherMapper.Map<ReadVoucherDto, Voucher>(x, creditNotes)).ToList();
            return new OkObjectResult(voucherDtos);
        }

        [HttpGet]
        [Route("invoice/list/all")]
        public async Task<ObjectResult> FindAllInvoices(DateTime start, DateTime end, bool ignoreSubItems = false, bool? isClosed = null, bool? isDue = null)
        {
            var vouchers = await _voucherRepository.FindInvoiceBases<InvoiceBase>(new DateInterval(start, end, true), ignoreSubItems, isClosed, isDue);
            var creditNotes = vouchers.OfType<CreditNote>().ToList();
            var invoices = vouchers.OfType<Invoice>().ToList();
            var invoiceDtos = invoices.Select(x => _voucherMapper.Map<InvoiceDto, Invoice>(x, creditNotes)).ToList();
            return new OkObjectResult(invoiceDtos);
        }

        [HttpGet]
        [Route("invoicebase/list/all")]
        public async Task<ObjectResult> FindAllInvoiceBases(DateTime start, DateTime end, bool ignoreSubItems = false, bool? isClosed = null, bool? isDue = null)
        {
            var vouchers = await _voucherRepository.FindInvoiceBases<InvoiceBase>(new DateInterval(start, end, true), ignoreSubItems, isClosed, isDue);
            var creditNotes = vouchers.OfType<CreditNote>().ToList();
            var invoiceBaseDtos = vouchers.Select(x => _voucherMapper.Map<InvoiceBaseDto, InvoiceBase>(x, creditNotes)).ToList();
            return new OkObjectResult(invoiceBaseDtos);
        }

        private void PopulateIsCreditedOnInvoiceDto(InvoiceDto invoiceDto)
        {
            invoiceDto.IsCredited = _voucherRepository.FindSingleOrDefaultCreditNoteFromInvoiceId(invoiceDto.Id) != null;
        }

        [HttpGet]
        [Route("payment/list/all")]
        public async Task<ObjectResult> FindAllPayments(DateTime start, DateTime end, bool? isClosed = null)
        {
            var vouchers = await _voucherRepository.FindAll<Payment>(new DateInterval(start, end, true), isClosed);
            var voucherDtos = vouchers.Select(x => _voucherMapper.Map<PaymentDto, Payment>(x, null)).ToList();
            return new OkObjectResult(voucherDtos);
        }

        [HttpGet]
        [Route("duedeposit/list/all")]
        public async Task<ObjectResult> FindAllDueDeposits(DateTime start, DateTime end, bool? isClosed = null, bool? isDue = null)
        {
            var dueVouchers = await _voucherRepository.FindAllDueVouchers<DueDeposit>(new DateInterval(start, end, true), isClosed, isDue);
            var depositDtos = dueVouchers.Select(x => _voucherMapper.Map<DueDepositDto, DueDeposit>(x, new List<CreditNote>())).ToList();
            return new OkObjectResult(depositDtos);
        }

        [HttpGet]
        [Route("invoice/order")]
        public async Task<ObjectResult> FindInvoiceFromOrder([Required(AllowEmptyStrings = false)] string orderId, bool ignoreSubItems = false)
        {
            return await FindInvoiceBasesFromOrderId<Invoice, InvoiceDto>(orderId, ignoreSubItems);
        }

        [HttpGet]
        [Route("invoicebase/list/all/order")]
        public async Task<ObjectResult> FindInvoiceBaseFromOrder([Required(AllowEmptyStrings = false)] string orderId, bool ignoreSubItems = false)
        {
            return await FindInvoiceBasesFromOrderId<InvoiceBase, InvoiceBaseDto>(orderId, ignoreSubItems);
        }

        [HttpGet]
        [Route("payment/list/all/order")]
        public async Task<ObjectResult> FindPaymentsFromOrder([Required(AllowEmptyStrings = false)] string orderId)
        {
            bool isFriendly = long.TryParse(orderId, out var friendlyId);
            List<Payment> payments;

            if (isFriendly)
            {
                payments = await _voucherRepository.FindPaymentsFromFriendlyOrderId(friendlyId);
            }
            else
            {
                var orderIdGuid = Guid.Parse(orderId);
                payments = await _voucherRepository.FindPaymentsFromOrderId(orderIdGuid);
            }

            var paymentDtos = _voucherMapper.Map<PaymentDto, Payment>(payments, null).ToList();
            return new ObjectResult(paymentDtos);
        }

        [HttpGet]
        [Route("paymentandcreditnote/list/all/order")]
        public async Task<ObjectResult> FindPaymentsOrCreditNotesFromOrder([NoEmpty] Guid orderId, bool? isClosed = null)
        {
            var paymentsOrCreditNotes = await _voucherRepository.FindPaymentsOrCreditNotesFromOrder(orderId, isClosed);
            var paymentsOrCreditNoteDtos = _voucherMapper.Map<ReadVoucherDto, Voucher>(paymentsOrCreditNotes, null).ToList();
            return new ObjectResult(paymentsOrCreditNoteDtos);
        }

        [HttpGet]
        [Route("payment/list/specific/invoice")]
        public async Task<ObjectResult> FindSpecificPaymentsFromInvoiceId([NoEmpty] Guid invoiceId)
        {
            var specificPayments = await this._voucherRepository.FindSpecificPaymentsFromInvoiceId(invoiceId, null);

            var paymentDtos = _voucherMapper.Map<PaymentDto, Payment>(specificPayments, null).ToList();
            return new ObjectResult(paymentDtos);
        }

        [HttpGet]
        [Route("creditnote/specific/invoice")]
        public async Task<ObjectResult> FindSpecificCreditNoteFromInvoiceId([NoEmpty] Guid invoiceId)
        {
            var creditNote = this._voucherRepository.FindSingleOrDefaultCreditNoteFromInvoiceId(invoiceId);

            if (creditNote == null) return new OkObjectResult(null);
            var creditNoteDto = _mapper.Map<CreditNoteDto>(creditNote);
            return new ObjectResult(creditNoteDto);
        }

        [HttpGet]
        [Route("creditnote/list/specific/closedtransaction")]
        public async Task<ObjectResult> FindSpecificCreditNotesFromVoucherTransactionId([NoEmpty] Guid transactionId)
        {
            var vouchers = await this._voucherRepository.FindVouchersFromClosedTransactionId(transactionId);
            var creditNotes = vouchers.OfType<CreditNote>();
            var creditNoteDto = creditNotes.Select(x => _mapper.Map<CreditNote, CreditNoteDto>(x)).ToList();
            return new ObjectResult(creditNoteDto);
        }

        [HttpGet]
        [Route("list/all/order")]
        public async Task<ObjectResult> FindVouchersFromOrderId([Required(AllowEmptyStrings = false)] string orderId)
        {
            return await FindVouchersFromOrderId<Voucher, VoucherDto>(orderId);
        }

        [HttpGet]
        [Route("readvoucher/list/all/order")]
        public async Task<ObjectResult> FindReadVoucherInvoiceBasesFromOrderId([Required(AllowEmptyStrings = false)] string orderId, bool ignoreSubItems = true)
        {
            return await FindReadVoucherInvoiceBasesFromOrderId<InvoiceBase>(orderId, ignoreSubItems);
        }

        [HttpGet]
        [Route("duedeposit/list/all/order")]
        public async Task<ObjectResult> FindDueDepositsFromOrderId([Required(AllowEmptyStrings = false)] string orderId)
        {
            return await this.FindVouchersFromOrderId<DueDeposit, DueDepositDto>(orderId);
        }

        [HttpGet]
        [Route("readvoucher/duedeposit/list/all/order")]
        public async Task<ObjectResult> FindReadVoucherDueDepositsFromOrderId([Required(AllowEmptyStrings = false)] string orderId)
        {
            var result = await this.FindVouchersFromOrderId<DueDeposit, ReadVoucherDto>(orderId);
            return result;
        }

        private async Task<ObjectResult> FindVouchersFromOrderId<TInvoiceType, TInvoiceTypeDto>(string orderId) where TInvoiceType : Voucher where TInvoiceTypeDto : VoucherDto
        {
            bool isFriendly = long.TryParse(orderId, out var friendlyId);
            List<TInvoiceType> invoiceBases;

            if (isFriendly)
            {
                invoiceBases = await _voucherRepository.FindFromFriendlyOrderId<TInvoiceType>(friendlyId, null);
            }
            else
            {
                invoiceBases = await _voucherRepository.FindFromOrderId<TInvoiceType>(Guid.Parse(orderId), null);
            }

            var creditNotes = invoiceBases.OfType<CreditNote>().ToList();
            var vouchers = _voucherMapper.Map<TInvoiceTypeDto, TInvoiceType>(invoiceBases, creditNotes).ToList();

            return new ObjectResult(vouchers);
        }

        private async Task<ObjectResult> FindReadVoucherInvoiceBasesFromOrderId<TInvoiceType>(string orderId, bool ignoreSubItems) where TInvoiceType : InvoiceBase
        {
            bool isFriendly = long.TryParse(orderId, out var friendlyId);
            List<TInvoiceType> invoiceBases;
            List<Payment> payments;

            if (isFriendly)
            {
                invoiceBases = await _voucherRepository.FindInvoiceBasesFromFriendlyOrderId<TInvoiceType>(friendlyId, ignoreSubItems);
                payments = await _voucherRepository.FindPaymentsFromFriendlyOrderId(friendlyId);
            }
            else
            {
                var orderIdGuid = Guid.Parse(orderId);
                invoiceBases = await _voucherRepository.FindInvoiceBasesFromOrderId<TInvoiceType>(orderIdGuid, ignoreSubItems);
                payments = await _voucherRepository.FindPaymentsFromOrderId(orderIdGuid);
            }

            var creditNotes = invoiceBases.OfType<CreditNote>().ToList();
            var invoiceBaseVouchers = _voucherMapper.Map<ReadVoucherDto, TInvoiceType>(invoiceBases, creditNotes).ToList();
            var paymentVouchers = _voucherMapper.Map<ReadVoucherDto, Payment>(payments, null).ToList();

            var vouchers = invoiceBaseVouchers.Concat(paymentVouchers).OrderBy(x => x.CreateDate).ToList();

            return new ObjectResult(vouchers);
        }

        private async Task<ObjectResult> FindInvoiceBasesFromOrderId<TInvoiceType, TInvoiceTypeDto>(string orderId, bool ignoreSubItems) where TInvoiceType : InvoiceBase where TInvoiceTypeDto : InvoiceBaseDto
        {
            bool isFriendly = long.TryParse(orderId, out var friendlyId);
            List<TInvoiceType> invoiceBases;

            if (isFriendly)
            {
                invoiceBases = await _voucherRepository.FindInvoiceBasesFromFriendlyOrderId<TInvoiceType>(friendlyId, ignoreSubItems);
            }
            else
            {
                invoiceBases = await _voucherRepository.FindInvoiceBasesFromOrderId<TInvoiceType>(Guid.Parse(orderId), ignoreSubItems);
            }

            var creditNotes = invoiceBases.OfType<CreditNote>().ToList();
            var invoiceBaseDtos = _voucherMapper.Map<TInvoiceTypeDto, TInvoiceType>(invoiceBases, creditNotes).ToList();

            return new ObjectResult(invoiceBaseDtos);
        }

        [HttpPost]
        [Route("invoice/credit")]
        public async Task<ObjectResult> CreditInvoice([NoEmpty] Guid id, DateTime? voucherDate = null)
        {
            using var transactionScope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.Snapshot }, TransactionScopeAsyncFlowOption.Enabled);
            var newCreditedInvoiceId = Guid.NewGuid();

            var creditNote = await _invoiceService.Credit(id, newCreditedInvoiceId, voucherDate ?? DateTime.Today);
            _voucherRepository.Add(creditNote);
            await _appDbContext.SaveChangesAsync();

            var creditNoteDto = this._voucherMapper.Map<CreditNoteDto, CreditNote>(creditNote, new List<CreditNote>());
            transactionScope.Complete();

            await _voucherClient.BroadcastDueInvoicesCount();
            return new CreatedResult(new Uri("?id=" + creditNote.Id, UriKind.Relative), creditNoteDto);
        }

        [HttpPost]
        [Route("payment/credit")]
        public async Task<ObjectResult> CreditPayment([NoEmpty] Guid id)
        {
            using var transactionScope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.Snapshot }, TransactionScopeAsyncFlowOption.Enabled);

            var creditedPayment = _paymentService.Credit(id);
            _voucherRepository.Add(creditedPayment);
            await _appDbContext.SaveChangesAsync();

            this._voucherMapper.Map<PaymentDto, Payment>(creditedPayment, new List<CreditNote>());

            var creditNoteDto = this._voucherMapper.Map<PaymentDto, Payment>(creditedPayment, new List<CreditNote>());
            transactionScope.Complete();

            await _voucherClient.BroadcastDueInvoicesCount();

            return new CreatedResult(new Uri("?id=" + creditedPayment.Id, UriKind.Relative), creditNoteDto);
        }

        [HttpPut]
        [Route("duedeposit/close")]
        public async Task<ObjectResult> CloseDueDeposit([NoEmpty] Guid id)
        {
            using var transactionScope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.Snapshot }, TransactionScopeAsyncFlowOption.Enabled);

            var dueDeposit = _voucherRepository.FindSingleVoucher<DueDeposit>(id);
            dueDeposit.Close();
            await _appDbContext.SaveChangesAsync();

            var dueDepositDto = this._voucherMapper.Map<DueDepositDto, DueDeposit>(dueDeposit, new List<CreditNote>());

            transactionScope.Complete();
            await _voucherClient.BroadcastDueDepositsCount();
            return new ObjectResult(dueDepositDto);
        }

        [HttpDelete]
        [Route("invoice/legacy/delete")]
        public async Task<IActionResult> DeleteLegacyInvoice([NoEmpty] Guid id)
        {
            using var transactionScope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.Snapshot }, TransactionScopeAsyncFlowOption.Enabled);

            Invoice deletedInvoice = await _invoiceService.DeleteLegacyInvoice(id);
            await _appDbContext.SaveChangesAsync();
            transactionScope.Complete();

            return new OkResult();
        }

        [Obsolete]
        [HttpGet]
        [Route("invoice/orderlineids/invoiced")]
        public async Task<ObjectResult> GetInvoicedOrderLineIds([NoEmpty] Guid orderId)
        {
            var orderLineIds = await _voucherRepository.GetInvoicedOrderLineIds(orderId);
            return new ObjectResult(orderLineIds);
        }

        [Obsolete]
        [HttpGet]
        [Route("invoice/orderlineids/invoiced/v2")]
        public async Task<ObjectResult> GetInvoicedOrderLineIds_v2([NoEmpty] Guid orderId)
        {
            var orderLineIds = await _voucherRepository.GetInvoicedOrderLinesInfo(orderId);
            return new ObjectResult(orderLineIds);
        }

        [HttpGet]
        [Route("filter/order/notinvoiced")]
        public async Task<ObjectResult> FilterNotInvoicedOrder([NoEmpty] Guid orderId)
        {
            var orderDto = await _orderClient.FindOrder(orderId);
            orderDto = await _invoiceService.RemoveInvoicedOrderLines(orderDto);
            return new ObjectResult(orderDto);
        }

        [HttpGet]
        [Route("invoice/due/count")]
        public async Task<ObjectResult> FindDueInvoicesCount()
        {
            var vouchers = await _voucherRepository.FindInvoiceBases<InvoiceBase>(DateInterval.Eternal, true, false, null);
            var invoices = vouchers.OfType<Invoice>().ToList();
            var invoicesDueCount = invoices.Count(x => x.IsDue);
            return new OkObjectResult(new SimpleJsonResult(invoicesDueCount));
        }


        [HttpGet]
        [Route("duedeposits/due/count")]
        public async Task<ObjectResult> FindDueDepositsCount()
        {
            Guid? tenantId = this.TryGetTenantId();
            _telemetryClient.TrackTrace("FindDueDepositsCount. tenantId = + tenantId");
            if (tenantId == null)
            {
                return new UnauthorizedObjectResult(null);
            }
            var deuDeposits = await _voucherRepository.FindAllDueVouchers<DueDeposit>(DateInterval.Eternal, false, null);
            var dueDepositsCount = deuDeposits.Count(x => x.IsDue);
            return new OkObjectResult(new SimpleJsonResult(dueDepositsCount));
        }


        [HttpPost]
        [Route("broadcast/dudepositscount")]
        public async Task BroadcastDueDepositsCount()
        {
            try
            {
                _appDbContext.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;

                var deuDeposits =
                    await _voucherRepository.FindAllDueVouchers<DueDeposit>(DateInterval.Eternal, false, null);
                var dueDepositsCount = deuDeposits.Count(x => x.IsDue);
                await _groupedBroadcasterClient.TryUpdateCounter("dueDeposits", dueDepositsCount, GetTenantId());
            }
            catch (Exception exception)
            {
                _telemetryClient.TrackException(exception);
            }
        }

        [HttpPost]
        [Route("broadcast/dueinvoicescount")]
        public async Task BroadcastDueInvoicesCount()
        {
            try
            {
                _appDbContext.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;

                var vouchers = await _voucherRepository.FindInvoiceBases<InvoiceBase>(DateInterval.Eternal, true, false, null);
                var invoices = vouchers.OfType<Invoice>().ToList();
                var invoicesDueCount = invoices.Count(x => x.IsDue);
                await _groupedBroadcasterClient.TryUpdateCounter("dueInvoices", invoicesDueCount, GetTenantId());
            }
            catch (Exception exception)
            {
                _telemetryClient.TrackException(exception);
            }
        }

        [HttpPost]
        [Route("broadcast/duevoucherscount")]
        public async Task BroadcastDueVouchersCount()
        {
            _appDbContext.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;

            await BroadcastDueDepositsCount();
            await BroadcastDueInvoicesCount();
        }

        //[HttpDelete]
        //[AllowAnonymous]
        //[Route("wrongpdf")]
        //public async Task<IActionResult> DeleteWrongPdf()
        //{
        //    using var transactionScope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.Snapshot }, TransactionScopeAsyncFlowOption.Enabled);

        //    var invoices = await _voucherRepository.FindInvoiceBasesGlobally<Invoice>(new DateTime(2020, 5, 18, 12, 0, 0), new DateTime(2020, 5, 18, 21, 0, 0));

        //    foreach (var invoice in invoices)
        //    {
        //        if (await _azureStorageExporter.ExistsFileInAzureStorageAsync("pdf", invoice.DocumentId.Value))
        //        {
        //            Debug.WriteLine("found invoice with id: " + invoice.FriendlyId);
        //            await _azureStorageExporter.DeleteFileInAzureStorageAsync("pdf", invoice.DocumentId.Value);
        //            Debug.WriteLine("deleted pdf for with id: " + invoice.DocumentId.Value);
        //        }
        //    }

        //    _appDbContext.SaveChanges();
        //    transactionScope.Complete();

        //    return new OkResult();
        //}
    }
}

