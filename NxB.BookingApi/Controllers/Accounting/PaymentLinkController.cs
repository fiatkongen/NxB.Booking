using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Transactions;
using System.Xml.Linq;
using AutoMapper;
using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Munk.AspNetCore;
using Munk.Utils.Object;
using Newtonsoft.Json;
using NxB.BookingApi.Exceptions;
using NxB.BookingApi.Infrastructure;
using NxB.BookingApi.Models;
using NxB.Domain.Common.Constants;
using NxB.Domain.Common.Enums;
using NxB.Domain.Common.Model;
using NxB.Dto.AccountingApi;
using NxB.Dto.Clients;
using NxB.Dto.LogApi;
using NxB.Dto.OrderingApi;
using NxB.Dto.TenantApi;
using NxB.Settings.Shared.Infrastructure;
using QuickPay.SDK.Models.Callbacks;
using QuickPay.SDK.Models.Payments;
using ServiceStack;
using JsonSerializer = System.Text.Json.JsonSerializer;
using Payment = NxB.BookingApi.Models.Payment;


namespace NxB.BookingApi.Controllers.Accounting
{
    [Produces("application/json")]
    [Microsoft.AspNetCore.Mvc.Route("paymentlink")]
    [Authorize]
    [ApiValidationFilter]
    public class PaymentLinkController : BaseController
    {
        private static string QUICKPAY_TYPE_AUTHORIZE = "authorize";
        private static string QUICKPAY_TYPE_CAPTURE = "capture";
        private static string QUICKPAY_TYPE_REFUND = "refund";
        private static string QUICKPAY_TYPE_CANCEL = "cancel";
        private static string QUICKPAY_STATE_PROCESSED = "processed";
        private static string QUICKPAY_STATE_NEW = "new";
        private static string QUICKPAY_STATE_PENDING = "pending";
        private static string QUICKPAY_STATE_REJECTED = "rejected";

        private readonly AppDbContext _appDbContext;
        private readonly IPaymentLinkService _paymentLinkService;
        private readonly IPaymentCompletionRepository _paymentCompletionRepository;
        private readonly IPaymentLinkRepository _paymentLinkRepository;
        private readonly IMapper _mapper;
        private readonly TelemetryClient _telemetryClient;
        private readonly ITenantClient _tenantClient;
        private readonly IVoucherRepository _voucherRepository;
        private readonly IInvoiceService _invoiceService;
        private readonly IOrderingService _orderingService;
        private readonly IEqualizeService _equalizeService;
        private readonly IBillingService _billingService;
        private readonly IGroupedBroadcasterClient _groupedBroadcasterClient;
        private readonly IOrderRepository _orderRepository;

        private DueVoucher _dueVoucher;
        private OrderDto _orderDto;
        private Payment _paymentAdded;

        public PaymentLinkController(AppDbContext appDbContext, IPaymentLinkService paymentLinkService, IMapper mapper, TelemetryClient telemetryClient, ITenantClient tenantClient, IVoucherRepository voucherRepository, IInvoiceService invoiceService, IEqualizeService equalizeService, IOrderingService orderingService, IPaymentCompletionRepository paymentCompletionRepository, IPaymentLinkRepository paymentLinkRepository, IBillingService billingService, IGroupedBroadcasterClient groupedBroadcasterClient, IOrderRepository orderRepository)
        {
            _appDbContext = appDbContext;
            _paymentLinkService = paymentLinkService;
            _mapper = mapper;
            _telemetryClient = telemetryClient;
            _tenantClient = tenantClient;
            _voucherRepository = voucherRepository;
            _invoiceService = invoiceService;
            _equalizeService = equalizeService;
            _orderingService = orderingService;
            _paymentCompletionRepository = paymentCompletionRepository;
            _paymentLinkRepository = paymentLinkRepository;
            _billingService = billingService;
            _groupedBroadcasterClient = groupedBroadcasterClient;
            _orderRepository = orderRepository;
        }

        [HttpPost]
        [Microsoft.AspNetCore.Mvc.Route("")]
        public async Task<ObjectResult> CreatePaymentLink([FromBody] CreatePaymentLinkDto createPaymentLinkDto)
        {
            var paymentLink = await _paymentLinkService.CreatePaymentLink(createPaymentLinkDto.FriendlyVoucherId, createPaymentLinkDto.VoucherType, createPaymentLinkDto.FriendlyOrderId, createPaymentLinkDto.Amount, createPaymentLinkDto.TestMode);
            await _appDbContext.SaveChangesAsync();

            var paymentLinkDto = _mapper.Map<PaymentLinkDto>(paymentLink);
            return new CreatedResult(new Uri("?id=" + paymentLinkDto.Id, UriKind.Relative), paymentLinkDto);
        }

        [HttpGet]
        [Microsoft.AspNetCore.Mvc.Route("")]
        public async Task<ObjectResult> FindPaymentLink(Guid id)
        {
            var paymentLink = await _paymentLinkRepository.FindSingleOrDefault(id);
            if (paymentLink == null) return new OkObjectResult(null);
            var paymentLinkDto = _mapper.Map<PaymentLinkDto>(paymentLink);
            return new OkObjectResult(paymentLinkDto);
        }

        [HttpPost]
        [AllowAnonymous]
        [Microsoft.AspNetCore.Mvc.Route("callback")]
        public async Task<IActionResult> CallbackFromQuickPay()
        {
            try
            {
                bool isDKCampCallback = false;
                string checkSum = Request.Headers["QuickPay-Checksum-Sha256"];
                string requestBody = await new StreamReader(Request.Body, Encoding.UTF8).ReadToEndAsync();
                _telemetryClient.TrackTrace("QuickPay.callback requestBody=" + requestBody);


                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    WriteIndented = true
                };
                
                if (requestBody.Contains("\"continue_url\":\"https://booking.dk-camp.dk/"))
                {
                    return await QuickPayDkCampCallback(requestBody, checkSum);
                }

                var callback = JsonSerializer.Deserialize<Callback>(requestBody, options);

                if (callback.Variables.ContainsKey("linkSource") && callback.Variables["linkSource"] == "onlinebooking")
                {
                    return await QuickPayOnlineCallback(requestBody, checkSum);
                }

                //ignore legacy callback
                if (!callback.Variables.ContainsKey("tenantId") || callback.OrderId.Contains("test_"))
                {
                    return new OkResult();
                }

                var testMode = callback.Variables.ContainsKey("testMode")
                    ? Enum.Parse<PaymentLinkTestMode>(callback.Variables["testMode"])
                    : PaymentLinkTestMode.None;

                if (testMode == PaymentLinkTestMode.NoCallback && !Debugger.IsAttached)
                {
                    _telemetryClient.TrackTrace("QuickPay.TestMode.NoCallback activated for " + requestBody);
                    return Ok();
                }

                if (!callback.Variables.ContainsKey("tenantId"))
                {
                    throw new PaymentCompletionException("QuickPay.CallbackFromQuickPay tenantId missing");
                }

                var tenantId = Guid.Parse(callback.Variables["tenantId"]);

                if (!callback.Variables.ContainsKey("paymentLinkId"))
                {
                    throw new PaymentCompletionException("QuickPay.CallbackFromQuickPay paymentLinkId missing");
                }

                var paymentLinkId = Guid.Parse(callback.Variables["paymentLinkId"]);

                if (!callback.Variables.ContainsKey("isFeeAdded"))
                {
                    throw new PaymentCompletionException("QuickPay.CallbackFromQuickPay isFeeAdded missing");
                }

                var isFeeAdded = bool.Parse(callback.Variables["isFeeAdded"]);

                var isAutoCaptured = false;
                if (!callback.Variables.ContainsKey("isAutoCaptured"))
                {
                    /*throw new PaymentCompletionException("QuickPay.CallbackFromQuickPay isAutoCaptured missing");*/
                }
                else
                {
                    isAutoCaptured = bool.Parse(callback.Variables["isAutoCaptured"]);
                }

                if (!callback.Variables.ContainsKey("voucherType"))
                {
                    throw new PaymentCompletionException(
                        "QuickPay.CallbackFromQuickPay voucherType missing. This is probably a refund for and old reservation. Do the refund manually");
                }

                var voucherType = (VoucherType)int.Parse(callback.Variables["voucherType"]);

                if (!callback.Variables.ContainsKey("friendlyOrderId"))
                {
                    throw new PaymentCompletionException(
                        "QuickPay.CallbackFromQuickPay friendlyOrderId missing. This is probably a refund for and old reservation. Do the refund manually");
                }

                var friendlyOrderId = long.Parse(callback.Variables["friendlyOrderId"]);

                if (!callback.Variables.ContainsKey("friendlyVoucherId"))
                {
                    throw new PaymentCompletionException("QuickPay.CallbackFromQuickPay friendlyVoucherId missing");
                }

                var friendlyVoucherId = long.Parse(callback.Variables["friendlyVoucherId"]);

                if (!callback.Variables.ContainsKey("amount"))
                {
                    throw new PaymentCompletionException("QuickPay.CallbackFromQuickPay amount missing");
                }

                var originalAmount = decimal.Parse(callback.Variables["amount"], CultureInfo.InvariantCulture);

                if (!Debugger.IsAttached && !_paymentLinkService.ValidateRequest(requestBody, checkSum, tenantId))
                    throw new PaymentCompletionException(
                        "QuickPay.CallbackFromQuickPay request does not conform to checksum");

                var temporaryClaimsProvider = new TemporaryClaimsProvider(tenantId, AppConstants.ADMINISTRATOR_ID,
                    "Administrator", null, null);
                var paymentCompletionRepository =
                    _paymentCompletionRepository.CloneWithCustomClaimsProvider(temporaryClaimsProvider);

                var transactionType = callback.Operations.Last().Type.Trim();
                _telemetryClient.TrackTrace("QuickPay.callback transactionType=" + transactionType);

                var currentState = callback.State.Trim();

                if (currentState == QUICKPAY_STATE_REJECTED && currentState != QUICKPAY_STATE_PENDING ||
                    (isAutoCaptured && transactionType == QUICKPAY_TYPE_AUTHORIZE))
                {
                    return new OkResult();
                }

                var orderRepository = _orderRepository.CloneWithCustomClaimsProvider(TemporaryClaimsProvider.CreateAdministrator(tenantId));
                var order = await orderRepository.FindSingleFromFriendlyId(friendlyOrderId, false);
                var orderId = order.Id;

                //already processed
                if (transactionType != QUICKPAY_TYPE_REFUND)
                {
                    var existingPaymentCompletion =
                        await paymentCompletionRepository.FindSingleOrDefaultFromQuickPayOrderId(callback.Id,
                            transactionType);
                    //already processed
                    if (existingPaymentCompletion != null)
                    {
                        return new OkResult();
                    }
                }

                if (string.IsNullOrEmpty(callback.OrderId))
                {
                    throw new PaymentCompletionException("QuickPay.CallbackFromQuickPay orderId missing");
                }

                await SignInFakeOnlineUserForTenant(_tenantClient, tenantId);

                var paymentCompletionId = Guid.NewGuid();

                if (!await ProcessAppendPaymentTransaction(requestBody, transactionType, originalAmount, orderId, tenantId,
                        null, paymentCompletionRepository, callback, paymentCompletionId, order, isFeeAdded,
                        isAutoCaptured, currentState, LinkSourceType.PaymentLink, paymentLinkId, friendlyVoucherId,
                        voucherType))
                {
                    return Ok();
                }

                ;

                if (transactionType == QUICKPAY_TYPE_CAPTURE)
                {
                    await _billingService.TryCreateBillableItem(new CreateBillableItemDto
                    {
                        Price = 1m,
                        Type = BillableItemType.PaymentLink,
                        Text = callback.OrderId,
                        Number = 1,
                        CreditPrice = 2,
                        BilledItemRef = paymentCompletionId,
                        AvoidDuplicateFromText = true
                    }, tenantId);
                }

                if (_paymentAdded != null)
                {
                    using (var transactionScope = new TransactionScope(TransactionScopeOption.Required,
                               new TransactionOptions { IsolationLevel = IsolationLevel.Snapshot },
                               TransactionScopeAsyncFlowOption.Enabled))
                    {
                        try
                        {
                            await _invoiceService.EqualizeVoucher(this._orderDto.Id,
                                new List<Guid> { this._dueVoucher.Id }, this._equalizeService, this._appDbContext);
                            transactionScope.Complete();
                        }
                        catch
                        {
                            transactionScope.Dispose();
                        }
                    }
                }

                return new OkResult();
            }
            catch
            {
                this.LogOnlineBookingFailedMetric();
                throw;
            }
        }

        private async Task<Payment> AppendPayment(long friendlyVoucherId, decimal amount, Guid tenantId, VoucherType voucherType)
        {
            _dueVoucher = _voucherRepository.FindSingleOrDefaultVoucherFromFriendlyId<DueVoucher>(friendlyVoucherId, voucherType);

            if (_dueVoucher == null)
            {
                throw new PaymentCompletionException($"QuickPay.CallbackFromQuickPay Could not find dueVoucher friendlyVoucherId={friendlyVoucherId}");
            }

            _orderDto = await _orderingService.FindOrder(_dueVoucher.OrderId, tenantId, true);
            var payment = await _invoiceService.CreateOnlinePayment(_orderDto, amount, _dueVoucher, _equalizeService, _appDbContext);
            return payment;
        }

        
        [HttpPost]
        [AllowAnonymous]
        [Microsoft.AspNetCore.Mvc.Route("callback/online")]
        public async Task<IActionResult> CallbackFromQuickPayOnline()
        {
            string checkSum = Request.Headers["QuickPay-Checksum-Sha256"];
            string requestBody = await new StreamReader(Request.Body, Encoding.UTF8).ReadToEndAsync();
            _telemetryClient.TrackTrace("QuickPayOnline.callback requestBody=" + requestBody);

            return await QuickPayOnlineCallback(requestBody, checkSum);
        }

        private async Task<IActionResult> QuickPayOnlineCallback(string requestBody, string checkSum)
        {
            try
            {
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    WriteIndented = true
                };
                var callback = JsonSerializer.Deserialize<Callback>(requestBody, options);

                var testMode = callback.Variables.ContainsKey("testMode")
                    ? Enum.Parse<PaymentLinkTestMode>(callback.Variables["testMode"])
                    : PaymentLinkTestMode.None;

                if (testMode == PaymentLinkTestMode.NoCallback && !Debugger.IsAttached)
                {
                    _telemetryClient.TrackTrace("QuickPayOnline.TestMode.NoCallback activated for " + requestBody);
                    return Ok();
                }


                if (!callback.Variables.ContainsKey("tenantId"))
                {
                    throw new PaymentCompletionException("QuickPayOnline.callbackFromQuickPayLegacy tenantId missing");
                }

                var tenantId = Guid.Parse(callback.Variables["tenantId"]);

                if (!callback.Variables.ContainsKey("orderId"))
                {
                    throw new PaymentCompletionException("QuickPayOnline.callbackFromQuickPayLegacy orderId missing");
                }

                var orderId = Guid.Parse(callback.Variables["orderId"]);

                if (!callback.Variables.ContainsKey("preferredLanguageId"))
                {
                    throw new PaymentCompletionException(
                        "QuickPayOnline.callbackFromQuickPayLegacy preferredLanguageId missing");
                }

                var preferredLanguageId = callback.Variables["preferredLanguageId"];

                if (!callback.Variables.ContainsKey("amount"))
                {
                    throw new PaymentCompletionException("QuickPayOnline.callbackFromQuickPayLegacy amount missing");
                }

                var originalAmount = decimal.Parse(callback.Variables["amount"], CultureInfo.InvariantCulture);

                if (!callback.Variables.ContainsKey("isFeeAdded"))
                {
                    throw new PaymentCompletionException("QuickPay.QuickPayOnline isFeeAdded missing");
                }

                var isFeeAdded = bool.Parse(callback.Variables["isFeeAdded"]);

                CartDto cartDto = null;
                if (callback.Variables.ContainsKey("cartJson") && callback.Variables["cartJson"] != null)
                {
                    var cartJson = callback.Variables["cartJson"];
                    _telemetryClient.TrackTrace($"QuickPayOnline.callback cartJson deserialized");

                    cartDto = JsonConvert.DeserializeObject<CartDto>(cartJson);
                    _telemetryClient.TrackTrace($"QuickPayOnline.callback cartJson deserialized");
                }


                if (!callback.Variables.ContainsKey("isAutoCaptured"))
                {
                    throw new PaymentCompletionException("QuickPay.QuickPayOnline isAutoCaptured missing");
                }

                var isAutoCaptured = bool.Parse(callback.Variables["isAutoCaptured"]);

                if (!Debugger.IsAttached && !_paymentLinkService.ValidateRequest(requestBody, checkSum, tenantId))
                    throw new PaymentCompletionException(
                        "QuickPayOnline.callbackFromQuickPayLegacy request does not conform to checksum");

                var temporaryClaimsProvider = TemporaryClaimsProvider.CreateOnline(tenantId);
                var paymentCompletionRepository =
                    _paymentCompletionRepository.CloneWithCustomClaimsProvider(temporaryClaimsProvider);

                var transactionType = callback.Operations.Last().Type.Trim();

                var currentState = callback.State.Trim();

                if (currentState == QUICKPAY_STATE_REJECTED && currentState != QUICKPAY_STATE_PENDING ||
                    (isAutoCaptured && transactionType == QUICKPAY_TYPE_AUTHORIZE))
                {
                    return new OkResult();
                }

                if (transactionType != QUICKPAY_TYPE_REFUND)
                {
                    var existingPaymentCompletion =
                        await paymentCompletionRepository.FindSingleOrDefaultFromQuickPayOrderId(callback.Id,
                            transactionType);
                    //already processed
                    if (existingPaymentCompletion != null)
                    {
                        return new OkResult();
                    }
                }

                if (string.IsNullOrEmpty(callback.OrderId))
                {
                    throw new PaymentCompletionException("QuickPayOnline.callbackFromQuickPayOnline orderId missing");
                }

                var orderRepository = _orderRepository.CloneWithCustomClaimsProvider(TemporaryClaimsProvider.CreateAdministrator(tenantId));
                var order = default(Order);

                var existsOrder = await orderRepository.Exists(orderId);
                if (!existsOrder && transactionType == QUICKPAY_TYPE_REFUND)
                {
                    //An onlinebooking went wrong and a refund was added, Just return OK
                    return Ok();
                }

                if (existsOrder)
                {
                    order = await orderRepository.FindSingle(orderId, false);
                }
                else
                {
                    if (cartDto == null)
                    {
                        throw new PaymentCaptureException("Cannot create onlinebooking when cartDto is null");
                    }

                    try
                    {
                        if (transactionType == QUICKPAY_TYPE_CANCEL)
                        {
                            return Ok();
                        }

                        string onlineTransactionDetails = ExtractOnlineTransactionDetails(callback, originalAmount);
                        cartDto.OnlineTransactionDetails = onlineTransactionDetails;
                        order = await orderClient.CreateOnlineOrder(cartDto, preferredLanguageId);

                        _telemetryClient.TrackTrace($"QuickPayOnline.callback order created");
                    }
                    catch (WebServiceException exception)
                    {
                        var exceptionResponseBody = exception.ResponseBody;
                        if (exceptionResponseBody.Contains("er ikke længere ledig") ||
                            exceptionResponseBody.Contains("er ikke ledig") ||
                            exceptionResponseBody.Contains("har 0 ledige enheder, men"))
                        {
                            var paymentCompletionException = new PaymentCompletionException(
                                $"QuickPayOnline.callbackFromQuickPayOnline order from cart could not be created. Trying to cancel or refund Transaction of type {transactionType}",
                                exception);

                            _telemetryClient.TrackException(paymentCompletionException,
                                new Dictionary<string, string> { { "WebException", exceptionResponseBody } });

                            if (transactionType == QUICKPAY_TYPE_AUTHORIZE || transactionType == QUICKPAY_TYPE_CANCEL)
                            {
                                _telemetryClient.TrackTrace(
                                    $"QuickPayOnline.callback Trying to cancel transaction type. Transactiontype is {transactionType}");
                                var quickPayClient = _paymentLinkService.CreateQuickPayClient(tenantId);
                                var result = await quickPayClient.Payments.Cancel(callback.Id).ConfigureAwait(false);
                                if (!result.Accepted)
                                {
                                    _telemetryClient.TrackTrace("QuickPayOnline.callback cancel error. json: " +
                                                                JsonConvert.SerializeObject(result));
                                    throw paymentCompletionException;
                                }

                                _telemetryClient.TrackTrace(
                                    $"Returning OK from PaymentLinkController, to stop retry. No booking has been created. Maybe try to 'refund' payment. Transactiontype is {transactionType}");
                                return Ok();
                            }

                            if (transactionType == QUICKPAY_TYPE_CAPTURE)
                            {
                                _telemetryClient.TrackTrace(
                                    $"QuickPayOnline.callback Trying to refund transaction type. Transactiontype is {transactionType}");
                                var quickPayClient = _paymentLinkService.CreateQuickPayClient(tenantId);
                                var result = await quickPayClient.Payments
                                    .Refund(callback.Id, Convert.ToInt32(originalAmount * 100)).ConfigureAwait(false);
                                if (!result.Accepted)
                                {
                                    _telemetryClient.TrackTrace("QuickPayOnline.callback refund error. json: " +
                                                                JsonConvert.SerializeObject(result));
                                    throw paymentCompletionException;
                                }

                                _telemetryClient.TrackTrace(
                                    $"Returning OK from PaymentLinkController, to stop retry. No booking has been created. Maybe try to 'refund' payment. Transactiontype is {transactionType}");
                                return Ok();
                            }

                            _telemetryClient.TrackTrace(
                                $"Cannot handle transaction type. Transactiontype is {transactionType}");

                            throw paymentCompletionException;
                        }
                        else
                        {
                            var paymentCompletionException = new PaymentCompletionException(
                                $"QuickPayOnline.callbackFromQuickPayOnline order from cart could not be created (Should the payment be cancelled?, and should an OK be returned to prevent further calls?)",
                                exception);
                            _telemetryClient.TrackException(paymentCompletionException,
                                new Dictionary<string, string> { { "WebException", exceptionResponseBody } });
                        }
                    }
                    catch (Exception ex)
                    {
                        //Should the payment be cancelled?, and should an OK be returned to prevent further calls?
                        var paymentCompletionException = new PaymentCompletionException(
                            $"QuickPayOnline.callbackFromQuickPayOnline order from cart could not be created (Should the payment be cancelled?, and should an OK be returned to prevent further calls?)",
                            ex);

                        throw paymentCompletionException;
                    }
                }

                await SignInFakeOnlineUserForTenant(_tenantClient, tenantId);


                var paymentCompletionId = Guid.NewGuid();

                if (!await ProcessAppendPaymentTransaction(requestBody, transactionType, originalAmount, orderId, tenantId,
                        preferredLanguageId, paymentCompletionRepository, callback, paymentCompletionId, order,
                        isFeeAdded,
                        isAutoCaptured, currentState, LinkSourceType.OnlineBooking, null, null, null))
                {
                    return Ok();
                }

                if (_paymentAdded != null && _dueVoucher != null)
                {
                    using (var transactionScope = new TransactionScope(TransactionScopeOption.Required,
                               new TransactionOptions { IsolationLevel = IsolationLevel.Snapshot },
                               TransactionScopeAsyncFlowOption.Enabled))
                    {
                        try
                        {
                            await _invoiceService.EqualizeVoucher(this._orderDto.Id, new List<Guid> { _dueVoucher.Id },
                                this._equalizeService, this._appDbContext);
                            transactionScope.Complete();
                        }
                        catch
                        {
                            transactionScope.Dispose();
                        }
                    }
                }

                return new OkResult();
            }
            catch
            {
                this.LogOnlineBookingFailedMetric();
                throw;
            }
        }

        private static string ExtractOnlineTransactionDetails(Callback callback, decimal originalAmount)
        {
            return $"QuickPay info \nGennemført af gæst: {callback.Created.ToEuTimeZoneFromUtc().ToDanishDateTime2()}\nBetalingsId: {callback.Id}\nBeløb {originalAmount.ToDanishDecimalString()} {callback.Currency}{(callback.TestMode ? "\nTestMode=Ja" : "")}\nKort anvendt: {callback.Metadata.Brand} ({callback.Metadata.Country}) - xxx{callback.Metadata.Last4}";
        }

        private async Task<bool> ProcessAppendPaymentTransaction(string requestBody, string transactionType, decimal originalAmount, Guid orderId,
            Guid tenantId, string preferredLanguageId, IPaymentCompletionRepository paymentCompletionRepository,
            Callback callback, Guid paymentCompletionId, OrderDto order, bool isFeeAdded, bool isAutoCaptured,
            string currentState, LinkSourceType linkSourceType, Guid? paymentLinkId, long? friendlyVoucherId, VoucherType? voucherType)
        {
            using (var transactionScope = new TransactionScope(TransactionScopeOption.Required,
                       new TransactionOptions { IsolationLevel = IsolationLevel.Snapshot },
                       TransactionScopeAsyncFlowOption.Enabled))
            {
                if (transactionType == QUICKPAY_TYPE_CAPTURE)
                {
                    _telemetryClient.TrackTrace($"QuickPayOnline.callback processing capture ");
                    var paymentAmount = originalAmount;

                    if (friendlyVoucherId == null)
                    {
                        _paymentAdded = await AppendPayment(orderId, paymentAmount, tenantId, preferredLanguageId);
                    }
                    else
                    {
                        if (voucherType == null)
                        {
                            throw new PaymentCaptureException("Vouchertype cannot be null");
                        }
                        _paymentAdded = await AppendPayment(friendlyVoucherId.Value, paymentAmount, tenantId, voucherType.Value);
                    }
                }

                if (transactionType == QUICKPAY_TYPE_AUTHORIZE)
                {
                    _telemetryClient.TrackTrace($"QuickPayOnline.callback processing Authorize ");
                }

                //this refund is called when refunding a payment from an onlinebooking
                decimal? overrideOriginalAmountOnPaymentCompletion = null;
                if (transactionType == QUICKPAY_TYPE_REFUND)
                {
                    _telemetryClient.TrackTrace(
                        $"QuickPayOnline.callback processing refund ");

                    var capturedPaymentCompletion =
                        await paymentCompletionRepository.FindSingleOrDefaultFromQuickPayPaymentId(callback.Id,
                            QUICKPAY_TYPE_CAPTURE);
                    var paymentAmount = 0 - (callback.Operations.Last().Amount.Value * 0.01m);
                    overrideOriginalAmountOnPaymentCompletion = paymentAmount;

                    if (capturedPaymentCompletion != null && capturedPaymentCompletion.OnCompletionAction ==
                        PaymentCompletionAction.AppendPayment)
                    {
                        _paymentAdded = await AppendPayment(orderId, paymentAmount, tenantId, preferredLanguageId);

                        if (_paymentAdded != null)
                        {
                            _telemetryClient.TrackTrace(
                                $"QuickPayOnline.callback refund på beløb {paymentAmount.ToDanishDecimalString()} added to order {_paymentAdded.FriendlyOrderId}, amount {_paymentAdded.Total}" +
                                transactionType);
                        }
                    }
                }

                var paymentCompletion = new PaymentCompletion(
                    paymentCompletionId,
                    tenantId,
                    overrideOriginalAmountOnPaymentCompletion ?? originalAmount,
                    _paymentAdded?.VoucherType,
                    order.FriendlyId,
                    _paymentAdded?.FriendlyId,
                    friendlyVoucherId,
                    callback.Id,
                    callback.OrderId,
                    callback.Accepted,
                    requestBody,
                    null,
                    isFeeAdded,
                    isAutoCaptured,
                    paymentLinkId,
                    currentState,
                    transactionType,
                    false,
                    false,
                    linkSourceType
                );

                if (transactionType == QUICKPAY_TYPE_CAPTURE && _paymentAdded != null)
                {
                    paymentCompletion.PaymentId = _paymentAdded.Id;
                }

                _telemetryClient.TrackTrace("QuickPayOnline.PaymentCompletion added " +
                                            JsonConvert.SerializeObject(paymentCompletion));

                await paymentCompletionRepository.Add(paymentCompletion);
                var lineId = transactionType == QUICKPAY_TYPE_REFUND ? callback.Operations.Last().Id.ToString() : null;

                EnsureCapturedPaymentIsNotAddedMultipleTimes(callback.Id, transactionType, lineId);

                try
                {
                    await _appDbContext.SaveChangesAsync();
                }
                catch (DbUpdateException dbUpdateException)
                {
                    if (dbUpdateException.InnerException != null &&
                        dbUpdateException.InnerException.Message.Contains(
                            "Violation of PRIMARY KEY constraint 'PK_PaymentCompletionLock'"))
                    {
                        _telemetryClient.TrackTrace(
                            "QuickPayOnline.PaymentCompletion same callback prevented from being performed twice");
                        _telemetryClient.TrackException(dbUpdateException);
                        return true;
                    }
                }

                await TryBroadcastActiveTransactions(paymentCompletionRepository, tenantId);
                transactionScope.Complete();
                _telemetryClient.TrackTrace("QuickPayOnline.callback completed for QuickpayId " + callback.Id);
            }

            return true;
        }

        private async Task<IActionResult> QuickPayDkCampCallback(string requestBody, string checkSum)
        {
            try
            {
                dynamic json = JsonConvert.DeserializeObject(requestBody);
                string externalOrderId = json.order_id;
                if (externalOrderId == null)
                {
                    throw new PaymentCompletionException("QuickPayDkCampCallback.callbackFromQuickPayLegacy externalOrderId missing");
                }

                dynamic variables = json.variables;
                variables.Remove("establishment_id");   //to avoid error
                requestBody = JsonConvert.SerializeObject(json);

                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    WriteIndented = true
                };
                var callback = JsonSerializer.Deserialize<Callback>(requestBody, options);

                var tenantId = await _orderRepository.FindTenantIdFromExternalOrderId(externalOrderId);
                if (tenantId == null)
                {
                    throw new PaymentCompletionException("QuickPayDkCampCallback.callbackFromQuickPayLegacy tenantId missing");
                }

                var transactionTypeDK = callback.Operations.Last().Type.Trim();
                _telemetryClient.TrackTrace("QuickPayDkCampCallback.callback transactionType=" + transactionTypeDK);

                //if (!callback.Variables.ContainsKey("preferredLanguageId"))
                //{
                //    throw new PaymentCompletionException(
                //        "QuickPayDkCampCallback.callbackFromQuickPayLegacy preferredLanguageId missing");
                //}

                var preferredLanguageId = callback.Variables["culture"]; //callback.Variables["preferredLanguageId"];

                var originalAmount = decimal.Parse(callback.Balance.ToString(), CultureInfo.InvariantCulture) * 0.01m;
                dynamic link = json.link;
                bool isFeeAdded = link.auto_fee;
                
                var isAutoCaptured = true;

                var temporaryClaimsProvider = TemporaryClaimsProvider.CreateOnline(tenantId.Value);
                var paymentCompletionRepository =
                    _paymentCompletionRepository.CloneWithCustomClaimsProvider(temporaryClaimsProvider);

                var transactionType = callback.Operations.Last().Type.Trim();

                var currentState = callback.State.Trim();

                if (currentState == QUICKPAY_STATE_REJECTED && currentState != QUICKPAY_STATE_PENDING ||
                    (isAutoCaptured && transactionType == QUICKPAY_TYPE_AUTHORIZE))
                {
                    return new OkResult();
                }

                if (transactionType != QUICKPAY_TYPE_REFUND)
                {
                    var existingPaymentCompletion =
                        await paymentCompletionRepository.FindSingleOrDefaultFromQuickPayOrderId(callback.Id,
                            transactionType);
                    //already processed
                    if (existingPaymentCompletion != null)
                    {
                        return new OkResult();
                    }
                }

                if (string.IsNullOrEmpty(callback.OrderId))
                {
                    throw new PaymentCompletionException("QuickPayOnline.callbackFromQuickPayOnline orderId missing");
                }

                await orderClient.AuthorizeClient(tenantId);
                OrderDto order = await orderClient.FindOrderFromExternalOrderId(externalOrderId);
                var existsOrder = order != null;
                if (!existsOrder && transactionType == QUICKPAY_TYPE_REFUND)
                {
                    //An onlinebooking went wrong and a refund was added, Just return OK
                    return Ok();
                }

                Guid orderId = order.Id;

                await SignInFakeOnlineUserForTenant(_tenantClient, tenantId.Value);

                var paymentCompletionId = Guid.NewGuid();

                if (!await ProcessAppendPaymentTransaction(requestBody, transactionType, originalAmount, orderId, tenantId.Value,
                        preferredLanguageId, paymentCompletionRepository, callback, paymentCompletionId, order,
                        isFeeAdded,
                        isAutoCaptured, currentState, LinkSourceType.DkCamp, null, null, null))
                {
                    return Ok();
                }

                if (_paymentAdded != null && _dueVoucher != null)
                {
                    using (var transactionScope = new TransactionScope(TransactionScopeOption.Required,
                               new TransactionOptions { IsolationLevel = IsolationLevel.Snapshot },
                               TransactionScopeAsyncFlowOption.Enabled))
                    {
                        if (_paymentAdded != null)  //hack Hvidesande gets payments that is referencing random invoices, this will probably happen to other as well (could be a counter hitting some valid number)
                        {
                            _paymentAdded.SpecificFriendlyInvoiceId = null;
                            _paymentAdded.SpecificInvoiceId = null;
                            _paymentAdded.UpdateText();
                        }
                        try
                        {
                            await _invoiceService.EqualizeVoucher(this._orderDto.Id, new List<Guid> { _dueVoucher.Id },
                                this._equalizeService, this._appDbContext);
                            transactionScope.Complete();
                        }
                        catch
                        {
                            transactionScope.Dispose();
                        }
                    }
                }

                string onlineTransactionDetails = ExtractOnlineTransactionDetails(callback, originalAmount);
                await orderClient.UpdateOrderOnlineTransactionDetails(new ModifyOrderOnlineTransactionDetails
                {
                    OrderId = order.Id,
                    TransactionDetails = onlineTransactionDetails
                });

                return new OkResult();
            }
            catch
            {
                this.LogOnlineBookingFailedMetric();
                throw;
            }
        }


        /// <summary>
        /// QuickPay sometimes sends same callback within a second
        /// (and the second check for the captured payment already added does not fail since the first payment is not yet committed ).
        /// This row in DB ensures that the payment is not added twice
        /// </summary>
        /// <param name="callback"></param>
        private void EnsureCapturedPaymentIsNotAddedMultipleTimes(int quickPayId, string transactionType, string idAppend)
        {
            _appDbContext.PaymentCapturedLocks.Add(new PaymentCompletedLock { QuickPayPaymentId = quickPayId, Action = transactionType + (idAppend ?? "") });
        }

        private void LogOnlineBookingFailedMetric()
        {
            var metrics = new Dictionary<string, double>
            {
                {"OnlineBooking Failure", 1},
            };

            _telemetryClient.TrackEvent("OnlineBookingMetrics", metrics: metrics);
        }

        private async Task<Payment> AppendPayment(Guid orderId, decimal amount, Guid tenantId, string preferredLanguageId)
        {
            var orderDto = await _orderingService.FindOrder(orderId, tenantId, true);

            var payment = await _invoiceService.CreatePayment(orderDto, amount, PaymentType.Online, preferredLanguageId + ",da", null, new List<Guid>(), new List<Guid>(), _equalizeService, null, _appDbContext, null);

            return payment;
        }

        public async Task TryBroadcastActiveTransactions(IPaymentCompletionRepository paymentCompletionRepository, Guid tenantId)
        {
            try
            {
                var count = await paymentCompletionRepository.CountActive();
                _telemetryClient.TrackTrace($"TryBroadcastActiveTransactions {count} for tenant: " + tenantId);
                await _groupedBroadcasterClient.TryUpdateCounter("activeTransactions", count, tenantId);
            }
            catch (Exception exception)
            {
                _telemetryClient.TrackException(exception);
            }
        }


    }
}

