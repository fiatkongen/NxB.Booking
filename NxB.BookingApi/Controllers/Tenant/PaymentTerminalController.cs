using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Munk.AspNetCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.ApplicationInsights;
using Newtonsoft.Json;
using NxB.Domain.Common.Enums;
using NxB.Dto.Clients;
using NxB.Dto.DocumentApi;
using NxB.BookingApi.Infrastructure;
using NxB.BookingApi.Models;
using NxB.Dto.TenantApi;

namespace NxB.BookingApi.Controllers.Tenant
{
    [Produces("application/json")]
    [Route("paymentterminal")]
    [Authorize]
    [ApiValidationFilter]
    public class PaymentTerminalController : BaseController
    {
        private static string PAYMENT_TRANSACTION_COUNTER_NAME = "PaymentTransactionId";

        private readonly IMapper _mapper;
        private readonly TelemetryClient _telemetry;
        private readonly AppDbContext _appDbContext;
        private readonly IExternalPaymentTransactionRepository _externalPaymentTransactionRepository;
        private readonly ExternalPaymentTransactionFactory _externalPaymentTransactionFactory;
        private readonly IVerifoneGateway _verifoneGateway;
        private readonly ICounterIdProvider _counterIdProvider;
        private readonly ITranslatorClient _translatorClient;

        public PaymentTerminalController(IMapper mapper, TelemetryClient telemetry, AppDbContext appDbContext, IExternalPaymentTransactionRepository externalPaymentTransactionRepository, ExternalPaymentTransactionFactory externalPaymentTransactionFactory, IVerifoneGateway verifoneGateway, ICounterIdProvider counterIdProvider, ITranslatorService translatorService, ITranslatorClient translatorClient)
        {
            _mapper = mapper;
            _telemetry = telemetry;
            _appDbContext = appDbContext;
            _externalPaymentTransactionRepository = externalPaymentTransactionRepository;
            _externalPaymentTransactionFactory = externalPaymentTransactionFactory;
            _verifoneGateway = verifoneGateway;
            _counterIdProvider = counterIdProvider;
            _translatorClient = translatorClient;
        }

        [HttpGet]
        [Route("status")]
        public async Task<ObjectResult> GetStatus()
        {
            var statusDto = await _verifoneGateway.GetStatus();
            return new ObjectResult(statusDto);
        }

        [HttpPost]
        [Route("abort")]
        public async Task<IActionResult> Abort(string terminalName, string saleId)
        {
            await _verifoneGateway.Abort(terminalName, saleId);
            return Ok();
        }

        [HttpPost]
        [Route("payment")]
        public async Task<ObjectResult> Payment([FromBody] PaymentTransactionRequestDto dto)
        {
            var transactionId = _counterIdProvider.Next_Shared(PAYMENT_TRANSACTION_COUNTER_NAME).ToString();
            var paymentResponse = await _verifoneGateway.Payment(dto.TerminalName, dto.Amount, dto.SaleId, transactionId);

            _externalPaymentTransactionRepository.Add(this._externalPaymentTransactionFactory.Create(
                transactionId,
                "payment",
                dto.Amount,
                paymentResponse.PaymentResponse.Response.Result,
                dto.VoucherId,
                JsonConvert.SerializeObject(paymentResponse),
                dto.SaleId
            ));

            await _appDbContext.SaveChangesAsync();

            var result = new PaymentTransactionResponseDto
            {
                Success = paymentResponse.PaymentResponse.Response.Result == "SUCCESS",
                Message = paymentResponse.PaymentResponse.Response.AdditionalResponse,
                TransactionId = transactionId
            };
            await TranslateMessage(result);
            return new CreatedResult("", result);
        }

        private async Task TranslateMessage(PaymentTransactionResponseDto result)
        {
            if (!string.IsNullOrWhiteSpace(result.Message))
            {
                if (result.Message != "INPROGRESS" && result.Message != "BUSY")
                {
                    result.Message = (await _translatorClient.TryTranslateText(new TranslationRequestDto
                        { SourceLanguage = "en", TargetLanguage = "da", Text = result.Message })).Text;
                }
            }
        }


        [HttpPost]
        [Route("refund")]
        public async Task<ObjectResult> Refund([FromBody] PaymentTransactionRequestDto dto)
        {
            var transactionId = _counterIdProvider.Next_Shared(PAYMENT_TRANSACTION_COUNTER_NAME).ToString();
            var paymentResponse = await _verifoneGateway.Refund(dto.TerminalName, dto.Amount, dto.SaleId, transactionId);

            _externalPaymentTransactionRepository.Add(this._externalPaymentTransactionFactory.Create(
                transactionId,
                "refund",
                dto.Amount,
                paymentResponse.PaymentResponse.Response.Result,
                dto.VoucherId,
                JsonConvert.SerializeObject(paymentResponse),
                dto.SaleId
            ));

            await _appDbContext.SaveChangesAsync();

            var result = new PaymentTransactionResponseDto
            {
                Success = paymentResponse.PaymentResponse.Response.Result == "SUCCESS",
                Message = paymentResponse.PaymentResponse.Response.AdditionalResponse,
                TransactionId = transactionId
            };
            await TranslateMessage(result);
            return new CreatedResult("", result);
        }

        [HttpGet]
        [Route("lasttransaction")]
        public async Task<ObjectResult> GetLastTransaction(string terminalName, string saleId)
        {
            var transaction = await _verifoneGateway.GetTransactionStatus(terminalName, saleId);
            var result = new PaymentTransactionResponseDto
            {
                Success = transaction.TransactionStatusResponse.RepeatedMessageResponse.RepeatedResponseMessageBody.PaymentResponse.Response.Result == "SUCCESS",
                Message = transaction.TransactionStatusResponse.RepeatedMessageResponse.RepeatedResponseMessageBody.PaymentResponse.Response.AdditionalResponse ?? transaction.TransactionStatusResponse.RepeatedMessageResponse.RepeatedResponseMessageBody.PaymentResponse.Response.ErrorCondition,
                TransactionId = transaction.TransactionStatusResponse.RepeatedMessageResponse.RepeatedResponseMessageBody.PaymentResponse?.SaleData.SaleTransactionID.TransactionID
            };
            await TranslateMessage(result);
            return new ObjectResult(result);
        }
    }
}