using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Newtonsoft.Json;
using NxB.Domain.Common.Enums;
using NxB.Dto.TenantApi;
using NxB.BookingApi.Exceptions;
using NxB.BookingApi.Models;
using System.IO;
using NxB.Settings.Shared.Infrastructure;

namespace NxB.BookingApi.Infrastructure
{
    public class VerifoneGateway : IVerifoneGateway
    {
        //private static string BASE_URL = "https://cstpos.test-gsc.vfims.com/oidc/poscloud/nexo/"; //test
        private static string BASE_URL = "https://emea-pos.gsc.verifone.cloud/oidc/poscloud/nexo/"; //prod lars


        private static string PAYMENT_TYPE_NORMAL = "NORMAL";
        private static string PAYMENT_TYPE_REFUND = "REFUND";

        private static string MESSAGE_CATEGORY_ABORT = "ABORT";
        private static string MESSAGE_CATEGORY_PAYMENT = "PAYMENT";
        private static string MESSAGE_CATEGORY_TRANSACTION_STATUS = "TRANSACTIONSTATUS";

        private static bool DEBUG_ON = false;

        private ISettingsRepository _settingsRepository;

        public VerifoneGateway(ISettingsRepository settingsRepository)
        {
            _settingsRepository = settingsRepository;
        }


        public async Task<VerifoneStatusDto> GetStatus()
        {
            using var client = await GetVerifoneClient("status", "status");
            var httpResponseMessage = await client.GetAsync("");

            var response = await httpResponseMessage.Content.ReadFromJsonAsync<VerifoneStatusDto>();
            return response;
        }

        public async Task<VerifonePaymentResponse> Payment(string POIID, decimal amount, string transactionId, string saleId, string currency = "DKK")
        {
            var transactionType = "payment";
            var client = await GetVerifoneClient(transactionType, "Payment");
            var paymentRequest = CreatePaymentRequest(amount, transactionId, currency, PAYMENT_TYPE_NORMAL);
            var verifonePaymentRequest = CreateVerifonePaymentRequest(POIID, MESSAGE_CATEGORY_PAYMENT, saleId, paymentRequest);

            Debug.WriteLine(JsonConvert.SerializeObject(verifonePaymentRequest));

            var httpResponseMessage = await client.PostAsJsonAsync(transactionType, verifonePaymentRequest);
            Debug.WriteLine(await httpResponseMessage.Content.ReadAsStringAsync());

            VerifonePaymentResponse response = null;
            try
            {
                response = await httpResponseMessage.Content.ReadFromJsonAsync<VerifonePaymentResponse>();
            }
            catch (Exception exception)
            {
                throw new PaymentTerminalException("Fejl på terminal ved betaling: " + exception.Message);
            }

            if (!httpResponseMessage.IsSuccessStatusCode)
            {
                throw new PaymentTerminalException("Fejl på terminal ved betaling. Http kode: " + httpResponseMessage.StatusCode);
            }
            return response;
        }

        public async Task<VerifonePaymentResponse> Refund(string POIID, decimal amount, string transactionId, string saleId, string currency = "DKK")
        {
            var client = await GetVerifoneClient("payment", "Refund");
            var paymentRequest = CreatePaymentRequest(amount, transactionId, currency, PAYMENT_TYPE_REFUND);
            var verifonePaymentRequest = CreateVerifonePaymentRequest(POIID, MESSAGE_CATEGORY_PAYMENT, saleId, paymentRequest);

            var httpResponseMessage = await client.PostAsJsonAsync("payment", verifonePaymentRequest);
            var response = await httpResponseMessage.Content.ReadFromJsonAsync<VerifonePaymentResponse>();

            if (!httpResponseMessage.IsSuccessStatusCode)
            {
                throw new PaymentTerminalException("Fejl på terminal ved refundering. Http kode: " + httpResponseMessage.StatusCode);
            }
            if (response.PaymentResponse.Response.Result == "FAILURE")
            {
                throw new PaymentTerminalException("Fejl på terminal ved refundering " + response.PaymentResponse.Response.ErrorCondition + " | " + response.PaymentResponse.Response.AdditionalResponse);
            }
            return response;
        }

        public async Task Abort(string POIID, string saleId)
        {
            var client = await GetVerifoneClient("abort", "Abort");
            var abortRequest = CreateAbortRequest();
            var verifoneAbortRequest = CreateVerifonePaymentAbortRequest(POIID, MESSAGE_CATEGORY_ABORT, saleId, abortRequest);

            var httpResponseMessage = await client.PostAsJsonAsync("abort", verifoneAbortRequest);
            if (!httpResponseMessage.IsSuccessStatusCode)
            {
                throw new PaymentTerminalException("Fejl på terminal ved annullering. Http kode: " + httpResponseMessage.StatusCode);
            }
        }

        public async Task<VerifoneTransactionStatusResponse> GetTransactionStatus(string POIID, string saleId)
        {
            var client = await GetVerifoneClient("transactionstatus", "TransactionStatus");
            var transactionStatusRequest = CreateTransactionStatusRequest();
            var verifoneTransactionStatusRequest = CreateVerifoneTransactionStatusRequest(POIID, MESSAGE_CATEGORY_TRANSACTION_STATUS, saleId, transactionStatusRequest);
            var httpResponseMessage = await client.PostAsJsonAsync("transactionstatus", verifoneTransactionStatusRequest);

            var response = await httpResponseMessage.Content.ReadFromJsonAsync<VerifoneTransactionStatusResponse>();

            if (!httpResponseMessage.IsSuccessStatusCode)
            {
                throw new PaymentTerminalException("Fejl på terminal ved hent af status. Http kode: " + httpResponseMessage.StatusCode);
            }

            return response;
        }

        private static VerifonePaymentRequest CreateVerifonePaymentRequest(string POIID, string messageCategory, string saleId, PaymentRequest paymentRequest)
        {
            return new VerifonePaymentRequest
            {
                MessageHeader = new MessageHeader
                {
                    MessageClass = "SERVICE",
                    MessageCategory = messageCategory,
                    MessageType = "REQUEST",
                    ServiceID = "NxB_POS",
                    SaleID = saleId,
                    POIID = POIID
                },
                PaymentRequest = paymentRequest
            };
        }

        private static PaymentRequest CreatePaymentRequest(decimal amount, string transactionId, string currency, string action)
        {
            if (currency == null) throw new ArgumentNullException(nameof(currency));
            if (action == null) throw new ArgumentNullException(nameof(action));

            return new PaymentRequest
            {
                SaleData = new SaleDataPaymentRequest
                {
                    OperatorID = null,
                    SaleTransactionID = new TransactionIDPaymentRequest
                    {
                        TransactionID = transactionId,
                        TimeStamp = DateTime.Now.ToJsonDateTimeString2()
                    },
                    CustomerOrderReq = new List<string> { "string" },
                    SaleToAcquirerData = "tender=MOTO"
                },
                PaymentTransaction = new PaymentTransaction
                {
                    AmountsReq = new AmountsReq
                    {
                        Currency = currency,
                        RequestedAmount = amount
                    }
                },
                PaymentData = new PaymentData
                {
                    PaymentType = action,
                    SplitPaymentFlag = false
                }
            };
        }

        private static VerifoneAbortRequest CreateVerifonePaymentAbortRequest(string POIID, string messageCategory, string saleId, AbortRequest abortRequest)
        {
            return new VerifoneAbortRequest
            {
                MessageHeader = new MessageHeader
                {
                    MessageClass = "SERVICE",
                    MessageCategory = messageCategory,
                    MessageType = "REQUEST",
                    ServiceID = "NxB_POS",
                    SaleID = saleId,
                    POIID = POIID
                },
                AbortRequest = abortRequest
            };
        }

        private static AbortRequest CreateAbortRequest(string reason = "CashierAborted")
        {
            return new AbortRequest
            {
                MessageReference = new MessageReference
                {
                    MessageCategory = "ABORT",
                    ServiceID = "string",
                    DeviceID = "string",
                    SaleID = "string",
                    POIID = "string"
                },
                AbortReason = reason,
                DisplayOutput = new DisplayOutput
                {
                    ResponseRequiredFlag = true,
                    MinimumDisplayTime = 0,
                    Device = "CASHIERDISPLAY",
                    InfoQualify = "STATUS",
                    OutputContent = new OutputContent
                    {
                        OutputFormat = "MESSAGEREF",
                        PredefinedContent = new PredefinedContent
                        {
                            Language = "string",
                            ReferenceID = "string"
                        },
                        OutputText = new List<OutputText>{ new()
                        {
                            Text = "string",
                            CharacterSet = 0,
                            Font = "string",
                            StartColumn = 0,
                            StartRow = 0,
                            Color = "WHITE",
                            CharacterWidth = "SINGLEWIDTH",
                            CharacterHeight = "SINGLEHEIGHT",
                            CharacterStyle = "NORMAL",
                            Alignment = "LEFT",
                            EndOfLineFlag = true
                        }},
                        OutputXHTML = "string",
                        OutputBarcode = new OutputBarcode
                        {
                            BarcodeType = "EAN8",
                            BarcodeValue = "string",
                            QRCodeBinaryValue = "string",
                            QRCodeErrorCorrection = "L",
                            QRCodeEncodingMode = "string",
                            QRCodeVersion = "string"
                        }
                    },
                    MenuEntry = new List<MenuEntry>{new() {
                        MenuEntryTag = "SELECTABLE",
                        OutputFormat = "MESSAGEREF",
                        DefaultSelectedFlag = true,
                        PredefinedContent = new PredefinedContent
                        {
                            ReferenceID = "string",
                            Language = "string"
                        },
                        OutputText = new List<OutputText>{new()
                        {
                            Text = "string",
                            CharacterSet = 0,
                            Font = "string",
                            StartColumn = 0,
                            StartRow = 0,
                            Color = "WHITE",
                            CharacterWidth = "SINGLEWIDTH",
                            CharacterHeight = "SINGLEHEIGHT",
                            CharacterStyle = "NORMAL",
                            Alignment = "LEFT",
                            EndOfLineFlag = true
                        }},
                        OutputXHTML = "string"
                    }},
                    OutputSignature = "string"
                }
            };
        }

        private static TransactionStatusRequest CreateTransactionStatusRequest()
        {
            return new TransactionStatusRequest
            {
                MessageReference = new MessageReference
                {
                    DeviceID = "string",
                    POIID = "string",
                    SaleID = "string",
                    ServiceID = "string",
                    MessageCategory = MESSAGE_CATEGORY_TRANSACTION_STATUS
                },
                ReceiptReprintFlag = false,
                DocumentQualifier = "SALERECEIPT"
            };
        }

        private static VerifoneTransactionStatusRequest CreateVerifoneTransactionStatusRequest(string POIID, string messageCategory, string saleId, TransactionStatusRequest transactionStatusRequest)
        {
            return new VerifoneTransactionStatusRequest
            {
                MessageHeader = new MessageHeader
                {
                    MessageClass = "SERVICE",
                    MessageCategory = messageCategory,
                    MessageType = "REQUEST",
                    ServiceID = "NxB_POS",
                    SaleID = saleId,
                    POIID = POIID
                },
                TransactionStatusRequest = transactionStatusRequest
            };
        }

        private async Task<HttpClient> GetVerifoneClient(string path, string context)
        {
            if (DEBUG_ON && Debugger.IsAttached)
            {
                var httpClient = new HttpClient(new LoggingHandler(new HttpClientHandler(), context))
                {
                    BaseAddress = new Uri(BASE_URL + path)
                };
                SetAuthHeader(httpClient);

                return httpClient;
            }
            else
            {
                var httpClient = new HttpClient()
                {
                    BaseAddress = new Uri(BASE_URL + path)
                };
                SetAuthHeader(httpClient);

                return httpClient;
            }
        }

        private void SetAuthHeader(HttpClient httpClient)
        {
            //test
            //httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic",
            //    "OWQ2ODk1YzYtMDQ5NC00MGJhLTgwYTItNzZkNTM1MWY3MjIwOnNySGlEbnF1d2VHTFVVRnpCaHlKVU5hUXZ2dm5pbVZ2ak1xag==");

            //production
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic",
                "YTI2MjQyMjgtZjUwOC00MGY2LThiMDMtN2IwYmNlNDE4NDJkOlJ2dHVyWmRYem50YkNWYUVKam9PdGdKVnJnbVhrWHJiSUxVSw==");    //prod lars

            var xSite = _settingsRepository.GetVerifoneSettings().XSite;

            if (string.IsNullOrEmpty(xSite))
                throw new Exception("Need to set x-site in settings");

            httpClient.DefaultRequestHeaders.Add("x-site-entity-id", xSite);

            //httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic",
            //    "MDc1NzBhNTAtMjNkOC00YjA4LTk4MjMtZGUwOWExNDM1MjcxOkVTV0F1YUJ5REtmbURyRXdLdUVDVUR5b3ZESHZnU1JSUFJLYg==");    //prod tove rejkjær

            //httpClient.DefaultRequestHeaders.Add("x-terminal-simulator", "true");
        }
    }
}