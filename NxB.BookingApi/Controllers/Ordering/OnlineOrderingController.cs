using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Transactions;
using Azure;
using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Abstractions;
using Munk.Utils.Object;
using Newtonsoft.Json;
using NxB.Allocating.Shared.Model.Exceptions;
using NxB.Domain.Common.Constants;
using NxB.Domain.Common.Enums;
using NxB.Domain.Common.Interfaces;
using NxB.Domain.Common.Model;
using NxB.Dto.AccountingApi;
using NxB.Dto.AllocationApi;
using NxB.Dto.AutomationApi;
using NxB.Dto.Clients;
using NxB.Dto.DocumentApi;
using NxB.Dto.JobApi;
using NxB.Dto.LogApi;
using NxB.Dto.OrderingApi;
using NxB.MemCacheActor.Interfaces;
using NxB.BookingApi.Exceptions;
using NxB.BookingApi.Infrastructure;
using NxB.BookingApi.Models;
using NxB.Settings.Shared.Infrastructure;
using Polly;
using QuickPay.SDK;

namespace NxB.BookingApi.Controllers.Ordering
{
    [Produces("application/json")]
    [Route("orderonline")]
    [Authorize]
    public class OnlineOrderingController : BaseController
    {
        private readonly OrderFactory _orderFactory;
        private readonly IOrderRepository _orderRepository;
        private readonly AppDbContext _appDbContext;
        private readonly CartDtoToCreateMapper _cartDtoToCreateMapper;
        private readonly TelemetryClient _telemetryClient;
        private readonly IOrderValidator _orderValidator;
        private readonly IAllocationStateRepository _allocationStateRepository;
        private readonly ICounterPushUpdateService _counterPushUpdateService;
        private readonly IGroupedBroadcasterClient _groupedBroadcasterClient;
        private readonly ITenantClient _tenantClient;
        private readonly IAvailabilityClient _availabilityClient;
        private readonly ICustomerClient _customerClient;
        private readonly IRentalCategoryClientCached _rentalCategoryClient;
        private readonly IRentalUnitClientCached _rentalUnitClient;
        private readonly IJobDocumentClient _jobDocumentClient;
        private readonly IMessageClient _messageClient;
        private readonly ISettingsRepository _settingsRepository;
        private readonly IAccessClient _accessClient;
        private readonly IAccessGroupClient _accessGroupClient;
        private readonly IReportingClient _reportingClient;
        private readonly IAllocationStateClient _allocationStateClient;
        private readonly IMemCacheActor _memCacheActor;
        private readonly IApplicationLogClient _applicationLogClient;
        private readonly ILicensePlateAutomationClient _licensePlateAutomationClient;
        private readonly IDocumentClient _documentClient;
        private readonly ICustomerRepository _customerRepository;
        private readonly IPaymentCompletionRepository _paymentCompletionRepository;

        private readonly List<Guid> _notSpecifiedResourceIds = new();

        public OnlineOrderingController(OrderFactory orderFactory, IOrderRepository orderRepository,
            AppDbContext appDbContext, CartDtoToCreateMapper cartDtoToCreateMapper, TelemetryClient telemetryClient,
            IOrderValidator orderValidator, IAllocationStateRepository allocationStateRepository,
            ICounterPushUpdateService counterPushUpdateService, IGroupedBroadcasterClient groupedBroadcasterClient,
            ITenantClient tenantClient, IAvailabilityClient availabilityClient, ICustomerClient customerClient,
            IRentalCategoryClientCached rentalCategoryClient, IRentalUnitClientCached rentalUnitClient, IJobDocumentClient jobDocumentClient, IMessageClient messageClient, ISettingsRepository settingsRepository,
            IAccessClient accessClient, IAccessGroupClient accessGroupClient, IReportingClient reportingClient, IAllocationStateClient allocationStateClient, IMemCacheActor memCacheActor,
            IApplicationLogClient applicationLogClient, ILicensePlateAutomationClient licensePlateAutomationClient, IDocumentClient documentClient, ICustomerRepository customerRepository, IPaymentCompletionRepository paymentCompletionRepository)
        {
            _orderFactory = orderFactory;
            _orderRepository = orderRepository;
            _appDbContext = appDbContext;
            _cartDtoToCreateMapper = cartDtoToCreateMapper;
            _telemetryClient = telemetryClient;
            _orderValidator = orderValidator;
            _allocationStateRepository = allocationStateRepository;
            _counterPushUpdateService = counterPushUpdateService;
            _groupedBroadcasterClient = groupedBroadcasterClient;
            _tenantClient = tenantClient;
            _availabilityClient = availabilityClient;
            _customerClient = customerClient;
            _rentalCategoryClient = rentalCategoryClient;
            _rentalUnitClient = rentalUnitClient;
            _jobDocumentClient = jobDocumentClient;
            _messageClient = messageClient;
            _settingsRepository = settingsRepository;
            _accessClient = accessClient;
            _accessGroupClient = accessGroupClient;
            _reportingClient = reportingClient;
            _allocationStateClient = allocationStateClient;
            _memCacheActor = memCacheActor;
            _applicationLogClient = applicationLogClient;
            _licensePlateAutomationClient = licensePlateAutomationClient;
            _documentClient = documentClient;
            _customerRepository = customerRepository;
            _paymentCompletionRepository = paymentCompletionRepository;
        }

        [HttpGet]
        [AllowAnonymous]
        [Route("calculate/amountdue")]
        public async Task<decimal> CalculateOrderTotal(int friendlyOrderId, Guid tenantId)
        {
            var order = await FindSingleOrDefaulOrdertFromFriendlyId(friendlyOrderId, tenantId);
            var orderTotals = await CalculateOrderTotals(order.Id, tenantId);
            return orderTotals.GetAmountDue();
        }

        private async Task<Order> FindSingleOrDefaulOrdertFromFriendlyId(int friendlyOrderId, Guid tenantId)
        {
            return await _orderRepository.CloneWithCustomClaimsProvider(TemporaryClaimsProvider.CreateAdministrator(tenantId)).FindSingleOrDefaultFromFriendlyId(friendlyOrderId, false);
        }

        private static async Task<AccountTotalsDto> CalculateOrderTotals(Guid orderId, Guid tenantId)
        {
            var accountClient = new AccountClient(null);
            await accountClient.AuthorizeClient(tenantId);

            var orderTotal = await accountClient.CalculateOrderTotals(Guid.Empty, orderId);
            return orderTotal;
        }

        [HttpGet]
        [AllowAnonymous]
        [Route("order/match")]
        public async Task<MatchOrderResult> MatchOrder(int friendlyOrderId, Guid tenantId, string phone = null, string email = null, string licensePlate = null, bool demandLicensePlate = false)
        {
            bool isMatched = false;

            if (string.IsNullOrWhiteSpace(phone) && string.IsNullOrWhiteSpace(email))
            {
                throw new ArgumentException("Phone and email cannot both be empty");
            }
            var order = await FindSingleOrDefaulOrdertFromFriendlyId(friendlyOrderId, tenantId);
            if (order == null || order.SubOrders.Count == 0) return MatchOrderResult.NoMatch;

            await _customerClient.AuthorizeClient(tenantId);
            var customerDto = await _customerClient.FindCustomerFromAccountId(order.AccountId);
            if (customerDto == null) return MatchOrderResult.MatchedNoCustomer;

            if (!string.IsNullOrWhiteSpace(phone))
            {
                phone = phone.Replace(" ", "");
                if (phone.Length >= 8)
                {
                    var match = customerDto.PhoneEntries.Where(x => x.Number.Length >= 8).Any(x => x.Number[^8..] == phone[^8..]);
                    isMatched = match;
                }
            }

            if (!isMatched && !string.IsNullOrWhiteSpace(email))
            {
                email = email.Replace(" ", "").ToLower();
                var match = customerDto.EmailEntries.Any(x => x.Email.ToLower() == email);
                isMatched = match;
            }

            if (!isMatched)
            {
                return MatchOrderResult.NoMatch;
            }

            var automationSettingsDto = _settingsRepository.GetAutomationSettings(tenantId);

            if (automationSettingsDto.IsLicensePlateAutomationEnabled && demandLicensePlate)
            {
                //hack
                var firstMatchIndex = order.SubOrders.FindIndex(su => su.AllocationOrderLines.Any(x => x.Start == DateTime.Now.Date));
                if (firstMatchIndex >= 0)
                {
                    var subOrder = order.SubOrders[firstMatchIndex];

                    var licensePlateAutomationClient = new LicensePlateAutomationClient(null);
                    await licensePlateAutomationClient.AuthorizeClient(tenantId);

                    LicensePlateAccessDto existingLicense = null;
                    if (string.IsNullOrWhiteSpace(licensePlate))
                    {
                        existingLicense = await licensePlateAutomationClient.FindAccess(subOrder.Id.ToString());
                        if (existingLicense == null)
                        {
                            return MatchOrderResult.MatchedMissingLicensePlate;
                        }
                    }
                }
            }


            return await MatchArrivalAndAmount(tenantId, order);
        }

        private async Task<MatchOrderResult> MatchArrivalAndAmount(Guid tenantId, Order order)
        {
            var allocationStateRepository =
                _allocationStateRepository.CloneWithCustomClaimsProvider(TemporaryClaimsProvider.CreateAdministrator(tenantId));

            var allArrived = order.SubOrders.All(x =>
            {
                var status = allocationStateRepository.FindSingle(x.Id).ArrivalStatus;
                return status == ArrivalStatus.Arrived;
            });

            if (allArrived)
            {
                return MatchOrderResult.MatchedAlreadyArrived;
            }

            var allToday = order.SubOrders.All(x =>
            {
                if (x.Start == DateTime.Now.Date) return true;
                var status = allocationStateRepository.FindSingle(x.Id).ArrivalStatus;
                return status == ArrivalStatus.Arrived;
            });

            bool someToday = false;

            if (!allToday)
            {
                someToday = order.SubOrders.Any(x => x.Start == DateTime.Now.Date);
                if (!someToday)
                {
                    return MatchOrderResult.MatchedNotToday;
                }
            }


            var firstTooEarly = await GetFirstTooEarly(tenantId, order);
            if (firstTooEarly != null)
            {
                return MatchOrderResult.MatchedToEarly;
            }

            if (someToday)  //when last suborder is checked in, the amount should be paid. But for now, just check in
            {
                return MatchOrderResult.MatchedNoErrors;
            }

            var orderTotals = await CalculateOrderTotals(order.Id, tenantId);
            if (orderTotals.GetAmountDue() > 0)
            {
                return MatchOrderResult.MatchedAmountDue;
            }

            return MatchOrderResult.MatchedNoErrors;
        }

        [HttpGet]
        [AllowAnonymous]
        [Route("order/firsttooearly")]
        public async Task<int?> FindFirstTooEarly(int friendlyOrderId, Guid tenantId)
        {
            var order = await FindSingleOrDefaulOrdertFromFriendlyId(friendlyOrderId, tenantId);
            var minutes = await GetFirstTooEarly(tenantId, order);
            return minutes;
        }

        private async Task<int?> GetFirstTooEarly(Guid tenantId, Order order)
        {
            await _rentalCategoryClient.AuthorizeClient(tenantId);
            await _rentalUnitClient.AuthorizeClient(tenantId);

            foreach (var resourceId in order.SubOrders.SelectMany(x => x.AllocationOrderLines).Where(x => x.Start == DateTime.Now.Date).Select(x => x.ResourceId).Distinct())
            {
                var rentalUnit = await _rentalUnitClient.FindSingleOrDefault(resourceId);
                var rentalCategory = await _rentalCategoryClient.FindSingleOrDefault(rentalUnit.RentalCategoryId);
                var euTimeZone = DateTime.Now.ToEuTimeZone();
                var currentMin = (euTimeZone.Hour * 60) + euTimeZone.Minute;
                if (rentalCategory.CheckInMin > 0 && rentalCategory.CheckInMin > currentMin)
                {
                    return rentalCategory.CheckInMin;
                }
            }

            return null;
        }

        [HttpPost]
        [AllowAnonymous]
        [Route("order/checkin")]
        public async Task<IActionResult> CheckInOrderForToday(int friendlyOrderId, Guid tenantId, string licensePlate = null, bool demandLicensePlate = false)
        {
            var order = await FindSingleOrDefaulOrdertFromFriendlyId(friendlyOrderId, tenantId);
            var matchResult = await MatchArrivalAndAmount(tenantId, order);

            if (matchResult != MatchOrderResult.MatchedNoErrors)
            {
                throw new CheckInException($"Kan ikke tjekke ind. Fejlkode: {matchResult}. Henvend dig i receptionen");
            }

            licensePlate = licensePlate?.Replace("-", "");

            var automationSettingsDto = _settingsRepository.GetAutomationSettings(tenantId);

            if (!automationSettingsDto.IsLicensePlateAutomationEnabled || !demandLicensePlate)
            {
                await CheckInOrderForToday(tenantId, order);
                return Ok();
            }

            //hack
            var firstMatchIndex = order.SubOrders.FindIndex(su => su.AllocationOrderLines.Any(x => x.Start == DateTime.Now.Date));
            if (firstMatchIndex < 0) return Ok();

            var subOrder = order.SubOrders[firstMatchIndex];

            var licensePlateAutomationClient = new LicensePlateAutomationClient(null);
            await licensePlateAutomationClient.AuthorizeClient(tenantId);

            LicensePlateAccessDto existingLicense = null;
            if (string.IsNullOrWhiteSpace(licensePlate))
            {
                existingLicense = await licensePlateAutomationClient.FindAccess(subOrder.Id.ToString());
                if (existingLicense == null)
                {
                    throw new CheckInException("Der er ikke registreret en nummerplade. Indtast venligst en nummerplade");
                }
            }

            await CheckInOrderForToday(tenantId, order);

            if (existingLicense != null)
            {
                if (!string.IsNullOrWhiteSpace(licensePlate))
                {
                    throw new CheckInException($"Kan ikke registrere nummerplade {licensePlate}, nummerplade {string.Join(",", existingLicense.LicensePlates)} er allerede registreret");
                }
                return Ok();
            }

            try
            {

                var subOrderAllocation = subOrder.Allocations[0];

                var rentalCategoryClient = new RentalCategoryClient(null);
                await rentalCategoryClient.AuthorizeClient(tenantId);

                var rentalUnitClient = new RentalUnitClient(null);
                await rentalUnitClient.AuthorizeClient(tenantId);

                var rentalUnit = await rentalUnitClient.FindSingleOrDefault(subOrderAllocation.RentalUnitId);
                var rentalCategory = await rentalCategoryClient.FindSingleOrDefault(rentalUnit.RentalCategoryId);

                var customer = _customerRepository.FindSingleFromAccountId(order.AccountId);
                if (customer != null)
                {

                    var start = subOrderAllocation.Start.AddMinutes(rentalCategory.CheckInMin);
                    var end = subOrderAllocation.End.AddMinutes(rentalCategory.CheckOutMin);

                    await licensePlateAutomationClient.CreateLicensePlateAccess(new LicensePlateAccessDto
                    {
                        CustomerId = (int)customer.FriendlyId,
                        Start = start,
                        End = end,
                        CustomerName = customer.DisplayString,
                        FriendlyOrderId = (int)order.FriendlyId,
                        OrderId = order.Id,
                        Id = subOrder.Id.ToString(),
                        RentalUnitName = subOrderAllocation.RentalUnitName,
                        MaxCarsIn = 1,
                        LicensePlates = licensePlate.Split(',').ToList()
                    }, true);
                }
            }
            catch (Exception exception)
            {
                _telemetryClient.TrackTrace("Error creating licenseplateaccess");
                _telemetryClient.TrackException(exception);
            }


            return Ok();
        }

        [HttpPost]
        [AllowAnonymous]
        [Route("nodeposit")]
        public async Task<ObjectResult> CreateOrderOnlineWithNoDeposit([FromBody] CartDto cartDto, string language)
        {
            if (cartDto.TenantId != AppConstants.DEMONSTRATION_TENANT_ID && cartDto.Deposit > 0)
            {
                return new UnauthorizedObjectResult($"CreateOrderOnlineWithNoDeposit for {cartDto.TenantId} not possible for deposit larger than 0 ({cartDto.Deposit})");
            }

            return await CreateOrderOnline(cartDto, language, true);
        }

        [HttpPost]
        [AllowAnonymous]
        [Route("")]
        public async Task<ObjectResult> CreateOrderOnline([FromBody] CartDto cartDto, string language, bool isDemo = false)
        {
            if (Debugger.IsAttached)
            {
                //cartDto.Customer.Email = "rasmus@hovemunk.dk";
                //cartDto.Customer.Phone = "+4540815446";
            }

            _telemetryClient.TrackTrace("cartDto: " + JsonConvert.SerializeObject(cartDto));
            var tenantId = cartDto.TenantId;

            string type;

            switch (cartDto.CreatedBy)
            {
                case CreatedBy.System:
                case CreatedBy.OnlineBooking:
                    type = "online";
                    break;
                case CreatedBy.External:
                    type = "ctoutvert";
                    break;
                case CreatedBy.Kiosk:
                    type = "kiosk";
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            var filterRentalCategoryId = cartDto.BookingCartItems[0].RentalCategoryId;

            try
            {
                await _availabilityClient.AuthorizeClient(tenantId);
                var availabilityDtos = await _availabilityClient.GetRentalUnitsAvailabilityFiltered(cartDto.Start, cartDto.End, type,
                        filterRentalCategoryId);

                await ValidateAllocationResourcesWhereSpecified(cartDto, availabilityDtos);
                await UpdateAllocationResourcesWhereUnspecified(cartDto, availabilityDtos, language);

                await SignInFakeOnlineUserForTenant(_tenantClient, cartDto.TenantId);
            }
            catch (Exception exception)
            {
                _telemetryClient.TrackTrace($"Error finding resources for {type} booking");
                _telemetryClient.TrackException(exception);
                LogOnlineBookingCreatedMetric(false, false, cartDto.CreatedBy);
                throw;
            }

            Order order;
            CustomerDto customer;
            CreatedResult createdResult;
            OrderDto orderDto;
            Guid? documentTemplateSmsId = null;
            Guid? documentTemplateId;

            RentalUnitDto rentalUnit;
            RentalCategoryDto rentalCategory;
            MessageDto messageCreated = null;

            using (var transactionScope = new TransactionScope(TransactionScopeOption.Required,
                       new TransactionOptions { IsolationLevel = IsolationLevel.Serializable },
                       TransactionScopeAsyncFlowOption.Enabled))
            {
                var createCustomerDto = _cartDtoToCreateMapper.MapCustomer(cartDto);

                await _customerClient.AuthorizeClient(tenantId);
                customer = await _customerClient.CreateCustomer(createCustomerDto);

                var createOrderDto = _cartDtoToCreateMapper.MapOrder(cartDto, customer.Accounts.First().Id);
                order = _orderFactory.Create(cartDto.OrderId, createOrderDto, _allocationStateRepository,
                    createOrderDto.OverrideFriendlyId);
                _orderRepository.Add(order);
                order.CreatedBy = cartDto.CreatedBy;
                order.CreatedByExternalId = cartDto.CreatedByExternalId;
                order.ExternalId = cartDto.ExternalId;
                order.CreateNote = string.IsNullOrWhiteSpace(cartDto.Note) ? null : cartDto.Note;
                order.Note = string.IsNullOrWhiteSpace(cartDto.OnlineCreationErrors) ? null : cartDto.OnlineCreationErrors;
                order.NoteState = cartDto.NoteState;
                order.OnlineTransactionDetails = cartDto.OnlineTransactionDetails;

                try
                {
                    var retryPolicy = Policy.Handle<AvailabilityException>()
                        .WaitAndRetryAsync(retryCount: 3, sleepDurationProvider: _ => TimeSpan.FromSeconds(2),
                            onRetry: (exception, sleepDuration, attemptNumber, context) =>
                            {
                            });

                    await retryPolicy.ExecuteAsync(async () =>
                    {
                        await _orderValidator.ValidateOrderAndInitializeCaches(order);
                    });
                }
                catch (AvailabilityException availabilityException)
                {
                    LogOnlineBookingCreatedMetric(false, null, cartDto.CreatedBy);
                    return new BadRequestObjectResult(availabilityException.Message);
                }

                await _rentalCategoryClient.AuthorizeClient(tenantId);

                var allocationOrderLine = order.SubOrders.First().AllocationOrderLines.First();
                var rentalUnitId = allocationOrderLine.ResourceId;

                await _rentalUnitClient.AuthorizeClient(tenantId);
                rentalUnit = await _rentalUnitClient.FindSingleOrDefault(rentalUnitId);
                rentalCategory = await _rentalCategoryClient.FindSingleOrDefault(rentalUnit.RentalCategoryId);

                if (rentalCategory.OnlineVoucherTemplateId == null && rentalCategory.OnlineDocumentTemplateId == null)
                {
                    throw new CreateOnlineOrderException(
                        $"Ordre kunne ikke oprettes da der ikke er valgt et onlinebilag eller skabelon for {allocationOrderLine.Text}");
                }

                var sendSmsAfterOnlineBooking = cartDto.CreatedBy == CreatedBy.External || cartDto.CreatedBy == CreatedBy.Kiosk || _settingsRepository.SendSmsAfterOnlineBooking();
                documentTemplateId = rentalCategory.OnlineVoucherTemplateId ?? rentalCategory.OnlineDocumentTemplateId;

                if (sendSmsAfterOnlineBooking)
                {

                    switch (cartDto.CreatedBy)
                    {
                        case CreatedBy.System:
                            throw new ArgumentOutOfRangeException();
                        case CreatedBy.OnlineBooking:
                            documentTemplateSmsId = AppConstants.DOCUMENTTEMPLATE_SMS_ID_GUEST_HAS_ONLINEBOOKED;
                            break;
                        case CreatedBy.External:
                            documentTemplateSmsId = rentalCategory.OnlineCtoutvertSmsTemplateId ?? AppConstants.DOCUMENTTEMPLATE_SMS_ID_GUEST_HAS_ONLINEBOOKED;

                            //if (cartDto.ExternalPaymentType == "QuickPay")
                            //{
                            documentTemplateId = rentalCategory.OnlineCtoutvertTemplateId ?? documentTemplateId;
                            //}
                            //else
                            //{
                            //documentTemplateId = rentalCategory.OnlineCtoutvertTemplateId;
                            //}

                            if (documentTemplateId == null)
                            {
                                throw new Exception("Error creating online order for Ctoutvert. No template selected for " + cartDto.BookingCartItems[0].RentalCategoryName);
                            }
                            break;
                        case CreatedBy.Kiosk:
                            documentTemplateSmsId = rentalCategory.OnlineKioskSmsTemplateId ?? AppConstants.DOCUMENTTEMPLATE_SMS_ID_GUEST_HAS_ONLINEBOOKED;
                            documentTemplateId = rentalCategory.OnlineKioskTemplateId ?? documentTemplateId;
                            if (documentTemplateId == null)
                            {
                                throw new Exception("Error creating online order for Kiosk. No template selected for " + cartDto.BookingCartItems[0].RentalCategoryName);
                            }
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
                if (documentTemplateId == null)
                {
                    throw new Exception("Error creating online order. No template selected for " + cartDto.BookingCartItems[0].RentalCategoryName);
                }

                orderDto = _orderFactory.Map(order, _allocationStateRepository);
                createdResult = new CreatedResult(new Uri("?id=" + orderDto.Id, UriKind.Relative), orderDto);

                try
                {
                    await AddAccessCodesToSubOrder(orderDto, tenantId, cartDto.CreatedBy);
                }
                catch (Exception exception)
                {
                    _telemetryClient.TrackException(exception);
                }

                try
                {
                    await AddLinkedRentalUnitAccessGroupsToSubOrder(orderDto, tenantId, cartDto.CreatedBy);
                }
                catch (Exception exception)
                {
                    _telemetryClient.TrackException(exception);
                }

                await _appDbContext.SaveChangesAsync();
                //throw new NotImplementedException();
                transactionScope.Complete();
            }

            ApplicationLogType applicationLogType = ApplicationLogType.OnlineBooking;
            switch (cartDto.CreatedBy)
            {
                case CreatedBy.System:
                    break;
                case CreatedBy.OnlineBooking:
                    break;
                case CreatedBy.External:
                    applicationLogType = ApplicationLogType.External;
                    break;
                case CreatedBy.Kiosk:
                    applicationLogType = ApplicationLogType.Kiosk;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }


            await _documentClient.AuthorizeClient(tenantId);
            var document = await _documentClient.FindSingleDocumentTemplate(documentTemplateId.Value);

            if (document.DocumentTemplateType == DocumentTemplateType.Voucher)
            {
                var message = new CreateAndSendVoucherDto
                {
                    OrderId = order.Id,
                    CustomerId = customer.Id,
                    DocumentTemplateId = documentTemplateId,
                    Languages = language,
                    FriendlyOrderId = (int)order.FriendlyId,
                    SaveId = Guid.NewGuid(),
                    DocumentTemplateSmsId = documentTemplateSmsId,
                    ForceHideNameForResourceIds = _notSpecifiedResourceIds,
                    DueDate = DateTime.Now.AddDays(1).ToEuTimeZone().Date,
                    VoucherDate = DateTime.Now.ToEuTimeZone().Date,
                    AccountId = customer.Accounts.First().Id,
                    DueAmount = cartDto.Deposit
                };

                try
                {
                    await _jobDocumentClient.AuthorizeClient(tenantId);
                    _telemetryClient.TrackTrace("CreateAndSendVoucherDto:" + JsonConvert.SerializeObject(message));
                    messageCreated = await _jobDocumentClient.CreateAndSendVoucher(message);
                }
                catch (Exception exception)
                {
                    await LogOnlineBookingNotFatalError(cartDto, order, exception, _applicationLogClient, tenantId, applicationLogType);

                    try
                    {
                        await _jobDocumentClient.CreateAndSendDocument(message, true);
                    }
                    catch (Exception exception2)
                    {
                        _telemetryClient.TrackException(exception2);
                    }
                }
            }
            else
            {
                var message = new CreateAndSendDocumentDto
                {
                    OrderId = order.Id,
                    CustomerId = customer.Id,
                    DocumentTemplateId = documentTemplateId,
                    Languages = language,
                    FriendlyOrderId = (int)order.FriendlyId,
                    SaveId = Guid.NewGuid(),
                    DocumentTemplateSmsId = documentTemplateSmsId,
                    ForceHideNameForResourceIds = _notSpecifiedResourceIds,
                };

                try
                {
                    await _jobDocumentClient.AuthorizeClient(tenantId);
                    messageCreated = await _jobDocumentClient.CreateAndSendDocument(message);
                }
                catch (Exception exception)
                {
                    await LogOnlineBookingNotFatalError(cartDto, order, exception, _applicationLogClient, tenantId,
                        applicationLogType);

                    try
                    {
                        await _jobDocumentClient.CreateAndSendDocument(message, true);
                    }
                    catch (Exception exception2)
                    {
                        _telemetryClient.TrackException(exception2);
                    }
                }
                //var quickPayClient = CreateQuickPayClient(tenantId);

                //var paymentCompletion = new PaymentCompletionDto(
                //    paymentCompletionId,
                //    tenantId,
                //    overrideOriginalAmountOnPaymentCompletion ?? originalAmount,
                //    _paymentAdded?.VoucherType,
                //    order.FriendlyId,
                //    _paymentAdded?.FriendlyId,
                //    friendlyVoucherId,
                //    callback.Id,
                //    callback.OrderId,
                //    callback.Accepted,
                //    requestBody,
                //    null,
                //    isFeeAdded,
                //    isAutoCaptured,
                //    paymentLinkId,
                //    currentState,
                //    transactionType,
                //    false,
                //    false,
                //    linkSourceType
                //);
                // paymentCompletionClient.

            }

            LogOnlineBookingCreatedMetric(true, true, cartDto.CreatedBy);

            if (cartDto.CreatedBy == CreatedBy.Kiosk)
            {
                await CheckInOrderForToday(tenantId, order);
            }

            try
            {
                var hasGuestNotes = !string.IsNullOrWhiteSpace(cartDto.Note);
                if (hasGuestNotes)
                {
                    var guestNotes =
                        "<br><div style='font-weight: 700;'>Denne e-mail er autogeneret og er IKKE sendt af gæsten. Men ved at svare på den vil den blive sendt til gæsten</div><div>Noter fra gæst:</div> <div style='font-style: italic'>" +
                          WebUtility.HtmlEncode(order.CreateNote) + "</div>";

                    var content = "<div>OnlineBooking oprettet " + order.CreateDate.Value.ToDanishDateTime() +
                                  "</div><br>" + guestNotes;

                    await _messageClient.AuthorizeClient(tenantId);
                    await _messageClient.CreateIntegrationMessage(new CreateIntegrationMessageDto
                    {
                        OrderId = order.Id,
                        FriendlyOrderId = order.FriendlyId,
                        CustomerId = customer.Id,
                        FriendlyCustomerId = customer.FriendlyId,
                        Sender = customer.GetSuggestedEmailsAsString(),
                        Content = content,
                        CustomerSearch = customer.DisplayString,
                        Subject = "OnlineBooking: LÆS NOTER FRA GÆST"
                    });
                }
            }
            catch (Exception exception)
            {
                _telemetryClient.TrackException(new CreateOnlineOrderException(
                    $"Ordre {order.FriendlyId} er oprettet, men kunne ikke oprette besked med gæstens noter ",
                    exception));
            }

            try
            {
                //if (!isDemo)
                //{
                //    var digitalGuestState = cartDto.CreatedBy == CreatedBy.Kiosk
                //        ? DigitalGuestState.Arrived
                //        : DigitalGuestState.Expected;
                //    _digitalGuestClientHelper.TryCheckDigitalGuestCreateOrModifyFireAndForget(orderDto,
                //        orderDto.SubOrders, DigitalGuestAction.Create, this.GetTenantId(), digitalGuestState);
                //}

                await _groupedBroadcasterClient.AuthorizeClient(tenantId);
                await _groupedBroadcasterClient.TryOrderModified(order.Id);

                await _reportingClient.AuthorizeClient(tenantId); //is injected in counterpushupdateservice
                await _counterPushUpdateService.TryPushUpdateMissingArrivalsDeparturesCounter();
            }
            catch (Exception exception)
            {
                _telemetryClient.TrackException(exception);
            }

            await _memCacheActor.PublishOrderCreated(GetTenantId(), orderDto).CatchExceptionAndLogToTelemetry(_telemetryClient);

            try
            {
                switch (cartDto.CreatedBy)
                {
                    case CreatedBy.External:
                        applicationLogType = ApplicationLogType.External;
                        break;
                    case CreatedBy.Kiosk:
                        applicationLogType = ApplicationLogType.Kiosk;
                        break;
                }

                await _applicationLogClient.TryAppendLog(
                    new ApplicationLogDto(applicationLogType, cartDto.NoteState ? SeverityType.Warning : SeverityType.Information, LogVisibilityType.All, $"Onlinebooking oprettet [{cartDto.CreatedBy}{(cartDto.CreatedByExternalId != null ? " - " + cartDto.CreatedByExternalId : "")}{(cartDto.ExternalId != null ? ": " + cartDto.ExternalId : "")}]")
                    {
                        FriendlyOrderId = order.FriendlyId,
                        OrderId = order.Id,
                        Amount = cartDto.Deposit,
                        Amount2 = order.CalculateTotal(),
                    }, tenantId);
            }
            catch (Exception exception)
            {
                _telemetryClient.TrackException(exception);
            }

            if ((cartDto.ExternalPaymentType == "QuickPay" || cartDto.ExternalPaymentType == "Online" || cartDto.CreatedByExternalId == "ACSI Camping.Info Booking" || cartDto.CreatedByExternalId == "ACSI / CampingCard" || cartDto.CreatedByExternalId == "PiNCAMP (ADAC, ANWB, TCS)") && cartDto.ExternalPaymentAmount is > 0)
            {
                var voucherClient = new VoucherClient(null);
                await voucherClient.AuthorizeClient(tenantId);

                PaymentDto addedPayment = null;
                if (document.DocumentTemplateType == DocumentTemplateType.Voucher && messageCreated.FriendlyVoucherId != null)
                {
                    var paymentDto = new CreateSpecificPaymentDto
                    {
                        OrderId = order.Id,
                        Language = language,
                        PaymentAmount = cartDto.ExternalPaymentAmount.Value,
                        PaymentType = PaymentType.Online,
                        SpecificFriendlyInvoiceId = messageCreated.FriendlyVoucherId.Value,
                        SpecificInvoiceId = messageCreated.VoucherId.Value
                    };
                    addedPayment = await voucherClient.CreateSpecificPayment(paymentDto);
                }
                else
                {
                    var paymentDto = new CreatePaymentDto
                    {
                        OrderId = order.Id,
                        Language = language,
                        PaymentAmount = cartDto.ExternalPaymentAmount.Value,
                        PaymentType = PaymentType.Online,
                    };
                    addedPayment = await voucherClient.CreatePayment(paymentDto);
                }
            }

            var automationSettingsDto = _settingsRepository.GetAutomationSettings();
            var licensePlate = cartDto.Customer.LicensePlate;
            if (automationSettingsDto.IsLicensePlateAutomationEnabled && !string.IsNullOrWhiteSpace(licensePlate))
            {
                try
                {
                    await _licensePlateAutomationClient.AuthorizeClient(tenantId);
                    int subOrderCounter = 0;

                    var start = cartDto.Start.AddHours(cartDto.ArrivalTime.Hour).AddMinutes(cartDto.ArrivalTime.Minute);
                    var end = cartDto.End.AddMinutes(rentalCategory.CheckOutMin);

                    foreach (var bookingCartItem in cartDto.BookingCartItems)
                    {
                        await _licensePlateAutomationClient.CreateLicensePlateAccess(new LicensePlateAccessDto
                        {
                            CustomerId = (int)customer.FriendlyId,
                            Start = start,
                            End = end,
                            CustomerName = customer.DisplayString,
                            FriendlyOrderId = (int)order.FriendlyId,
                            OrderId = order.Id,
                            Id = order.SubOrders[subOrderCounter++].Id.ToString(),
                            RentalUnitName = bookingCartItem.RentalUnitName,
                            MaxCarsIn = 1,
                            LicensePlates = licensePlate.Split(',').ToList()
                        }, true);
                    }
                }
                catch (Exception exception)
                {
                    _telemetryClient.TrackTrace("Error creating licenseplateaccess");
                    _telemetryClient.TrackException(exception);
                }
            }

            var _nibeId = Guid.Parse("01e546b0-36dd-4720-9f67-1e2c49ec66af");
            // _demonstrationId = Guid.Parse("37A28F72-DBC1-4B8B-A4F9-F8C146002123");

            if (tenantId == _nibeId)
            {
                try
                {
                    var nibeTracking =
                        $"https://www.nibecamping.dk/kvittering?ordreid={order.FriendlyId}&value={(order.CalculateTotal() * 100m):F0}&currency=DKK&email={(customer.GetSuggestedEmails().FirstOrDefault() ?? "")}&phone={(customer.GetSuggestedPhoneEntries().FirstOrDefault()?.TotalNumber ?? "")}";

                    _telemetryClient.TrackTrace("nibe tracking: " + nibeTracking);

                    var nibeClient = new HttpClient();

                    // nibeClient.GetAsync(new Uri(nibeTracking)).FireAndForgetLogToTelemetry(_telemetryClient);
                    var response = await nibeClient.GetAsync(new Uri(nibeTracking));
                    var content = await response.Content.ReadAsStringAsync();
                    _telemetryClient.TrackTrace(content);
                }
                catch (Exception exception)
                {
                    _telemetryClient.TrackTrace("Error tracking nibe");
                    _telemetryClient.TrackException(exception);
                }
            }

            return createdResult;
        }

        private async Task LogOnlineBookingNotFatalError(CartDto cartDto, Order order, Exception exception, IApplicationLogClient applicationLogClient, Guid tenantId, ApplicationLogType applicationLogType)
        {
            LogOnlineBookingCreatedMetric(true, false, cartDto.CreatedBy);
            _telemetryClient.TrackException(new CreateOnlineOrderException(
                $"Dokument {order.FriendlyId} er oprettet, men kunne ikke sende e-mail. Send e-mail manuelt. " +
                $"Document {order.FriendlyId} is completed, but could not send e-mail. Send e-mail manually. ",
                exception));

            await applicationLogClient.TryAppendLog(
                new ApplicationLogDto(applicationLogType, SeverityType.Error, LogVisibilityType.All, "Mulig delvis fejl ved oprettelse af onlinebooking. Check sendt bekræftelse/opkrævning")
                {
                    FriendlyOrderId = order.FriendlyId,
                    OrderId = order.Id,
                    Amount = cartDto.Deposit,
                    Amount2 = order.CalculateTotal(),
                }, tenantId);
        }

        private async Task CheckInOrderForToday(Guid tenantId, Order order)
        {
            try
            {
                await _allocationStateClient.AuthorizeClient(tenantId);
                foreach (var subOrder in order.SubOrders.Where(su => su.AllocationOrderLines.Any(x => x.Start == DateTime.Now.Date)))
                {
                    await _allocationStateClient.AddArrivalState(new AddAllocationStateDto
                    {
                        CustomTime = DateTime.Now.ToEuTimeZone(),
                        Status = AllocationStatus.Arrived,
                        SubOrderId = subOrder.Id
                    });
                }
            }
            catch (Exception exception)
            {
                _telemetryClient.TrackException(exception);
            }
        }

        private async Task ValidateAllocationResourcesWhereSpecified(CartDto cartDto, List<AvailabilityDto> availabilityDtos)
        {
            var cartItemsWithResourceSpecified = cartDto.BookingCartItems.Where(x => x.RentalUnitId != null).ToList();
            if (cartItemsWithResourceSpecified.Count == 0) return;

            var rentalCategoryName = cartItemsWithResourceSpecified[0].RentalCategoryName;

            _telemetryClient.TrackTrace($"OnlineOrderingController.ValidateAllocationResourcesWhereSpecified {cartItemsWithResourceSpecified.Count} resources specified for {rentalCategoryName}. Checking if resource is still available");

            await _availabilityClient.AuthorizeClient(cartDto.TenantId);

            foreach (var cartItemDto in cartItemsWithResourceSpecified)
            {
                var resourceAvailable = availabilityDtos.FirstOrDefault(x => x.ResourceId == cartItemDto.RentalUnitId && x.Number >= 0);
                if (resourceAvailable != null)
                {
                    availabilityDtos.Remove(resourceAvailable);
                    _telemetryClient.TrackTrace($"OnlineOrderingController.ValidateAllocationResourcesWhereSpecified. Resource {cartItemDto.RentalCategoryName}({cartItemDto.RentalUnitId}) {cartDto.Start.ToDanishDateTime()}-{cartDto.End.ToDanishDateTime()} is available");
                }
                else
                {
                    throw new CreateOnlineOrderException($"Kan ikke oprette booking. Kategori {cartItemDto.RentalUnitName} ({rentalCategoryName}) er ikke længere ledig. Cannot create booking. Catagory {cartItemDto.RentalUnitName} ({rentalCategoryName}) is no longer available");
                }
            }
        }


        private async Task UpdateAllocationResourcesWhereUnspecified(CartDto cartDto, List<AvailabilityDto> availabilityDtos, string language)
        {
            var cartItemsWithNoResourceSpecified = cartDto.BookingCartItems.Where(x => x.RentalUnitId == null).ToList();
            if (cartItemsWithNoResourceSpecified.Count == 0) return;

            var rentalCategoryName = cartItemsWithNoResourceSpecified[0].RentalCategoryName;

            _telemetryClient.TrackTrace($"OnlineOrderingController.UpdateAllocationResourcesWhereUnspecified no resource specified for {rentalCategoryName}. Trying to allocation resource");
            await _availabilityClient.AuthorizeClient(cartDto.TenantId);

            if (availabilityDtos.Count < cartItemsWithNoResourceSpecified.Count) throw new CreateOnlineOrderException($"Kan ikke oprette booking. Udlejningskategori {rentalCategoryName} har {availabilityDtos.Count} ledige enheder, men {cartItemsWithNoResourceSpecified.Count} er nødvendige");

            foreach (var cartItemDto in cartItemsWithNoResourceSpecified)
            {
                if (availabilityDtos.Count == 1)
                {
                    cartItemDto.RentalUnitId = availabilityDtos[0].ResourceId;
                    availabilityDtos.Remove(availabilityDtos[0]);
                }
                else
                {
                    var randomUnitIndex = new Random().Next(availabilityDtos.Count);
                    cartItemDto.RentalUnitId = availabilityDtos[randomUnitIndex].ResourceId;
                    availabilityDtos.Remove(availabilityDtos[randomUnitIndex]);
                }

                if (cartDto.CreatedBy != CreatedBy.Kiosk)   //gust must know where to stay
                {
                    _notSpecifiedResourceIds.Add(cartItemDto.RentalUnitId.Value);
                }

                await _rentalUnitClient.AuthorizeClient(cartDto.TenantId);
                var rentalUnit = await _rentalUnitClient.FindSingleOrDefault(cartItemDto.RentalUnitId.Value);
                cartItemDto.RentalUnitName = rentalUnit.NameTranslations.TranslateWithFallback("s_" + language);
                if (string.IsNullOrEmpty(cartItemDto.RentalUnitName))
                {
                    _telemetryClient.TrackException(new CreateOnlineOrderException($"(internal) no name for rentalUnit {rentalUnit.Id}. Using language {language}"));
                }
                _telemetryClient.TrackTrace($"OnlineOrderingController.UpdateAllocationResourcesWhereUnspecified. Resource {cartItemDto.RentalUnitId} {cartDto.Start.ToDanishDateTime()}-{cartDto.End.ToDanishDateTime()} allocated");
            }
        }

        private async Task AddAccessCodesToSubOrder(OrderDto orderDto, Guid tenantId, CreatedBy createdBy)
        {
            if (createdBy == CreatedBy.System)
            {
                return;

            }

            foreach (var subOrder in orderDto.SubOrders)
            {
                var allocationOrderLineDto = subOrder.AllocationOrderLines.First();
                var rentalUnit = await _rentalUnitClient.FindSingleOrDefault(allocationOrderLineDto.ResourceId);
                var rentalCategoryDto = await _rentalCategoryClient.FindSingleOrDefault(rentalUnit.RentalCategoryId);
                Guid? accessGroupId = null;

                switch (createdBy)
                {
                    case CreatedBy.OnlineBooking:
                        accessGroupId = rentalCategoryDto.OnlineAccessGroupId;
                        break;
                    case CreatedBy.External:
                        break;
                    case CreatedBy.Kiosk:
                        accessGroupId = rentalCategoryDto.KioskAccessGroupId ?? rentalCategoryDto.OnlineAccessGroupId;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(createdBy), createdBy, null);
                }
                if (accessGroupId == null || accessGroupId.Value == Guid.Empty) { continue; }

                await CreateAccesses(tenantId, accessGroupId, subOrder);
            }
        }

        private async Task CreateAccesses(Guid tenantId, Guid? accessGroupId, SubOrderDto subOrder)
        {
            await _accessGroupClient.AuthorizeClient(tenantId);
            await _accessClient.AuthorizeClient(tenantId);

            var accessGroupDto = await _accessGroupClient.FindAccessGroup(accessGroupId.Value);
            var createAccessDto = new CreateOrModifyAccessFromAccessibleItemsDto
            {
                IsKeyCode = true,
                SubOrderId = subOrder.Id,
                AccessibleItems = new AccessibleItems
                {
                    AccessItems = new List<AccessItem>
                    {
                        new()
                        {
                            AccessGroupId = accessGroupId.Value,
                            Name = accessGroupDto.Name
                        }
                    }
                }
            };

            await _accessClient.CreateAccessToAccessibleItems(createAccessDto);
        }

        private async Task AddLinkedRentalUnitAccessGroupsToSubOrder(OrderDto orderDto, Guid tenantId, CreatedBy createdBy)
        {
            if (createdBy != CreatedBy.Kiosk)
            {
                return;
            }

            foreach (var subOrder in orderDto.SubOrders)
            {
                var allocationOrderLineDto = subOrder.AllocationOrderLines.First();
                var rentalUnit = await _rentalUnitClient.FindSingleOrDefault(allocationOrderLineDto.ResourceId);
                Guid? accessGroupId = rentalUnit.LinkAccessId;

                if (accessGroupId == null || accessGroupId.Value == Guid.Empty) { continue; }

                await CreateAccesses(tenantId, accessGroupId, subOrder);
            }
        }

        private void LogOnlineBookingCreatedMetric(bool bookingSuccess, bool? emailSuccess, CreatedBy createdBy)
        {
            var providerName = Enum.GetName(createdBy);

            var metrics = new Dictionary<string, double>
            {
                {providerName + " Success", bookingSuccess ? 1 : 0},
                {"OnlineBooking Failure", bookingSuccess ? 0 : 1},

            };
            if (emailSuccess != null)
            {
                if (emailSuccess.Value)
                {
                    metrics.Add("OnlineBooking Email Success", 1);
                }
                else
                {
                    metrics.Add("OnlineBooking Email Failure", 1);
                }
                ;
            }
            _telemetryClient.TrackEvent("OnlineBookingMetrics", metrics: metrics);
        }

        public QuickPayClient CreateQuickPayClient(Guid tenantId)
        {
            var quickPaySettings = _settingsRepository.GetQuickPaySettings(tenantId);
            return new QuickPayClient(quickPaySettings.QuickPayApiUser, quickPaySettings.QuickPayPrivateKey, quickPaySettings.QuickPayUserKey);
        }
    }
}
