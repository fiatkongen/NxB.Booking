using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using NxB.Domain.Common.Constants;
using NxB.Domain.Common.Interfaces;

namespace NxB.Clients
{
    public abstract class NxBClient
    {
        public bool IsAuthorized { get; set; }
        protected readonly IHttpContextAccessor _httpContextAccessor;
        protected HttpClient Client { get; set; }
        private readonly bool _useHttp = true;

        protected NxBClient(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
            InitializeClient();
        }

        protected NxBClient()
        {
            InitializeClient();
        }

        public virtual async Task GetAsync(string relativeOrAbsoluteUrl)
        {
            InitializeClient();
            await Client.GetAsync(Client.BaseAddress + relativeOrAbsoluteUrl);
        }

        public virtual async Task<TResponse> GetAsync<TResponse>(string relativeOrAbsoluteUrl)
        {
            InitializeClient();

            var response = await Client.GetAsync(Client.BaseAddress + relativeOrAbsoluteUrl);
            var responseContent = await response.Content.ReadAsStringAsync();

            // Check if the response content is empty
            if (string.IsNullOrWhiteSpace(responseContent))
            {
                return default(TResponse);
            }

            // Deserialize the response content
            return JsonConvert.DeserializeObject<TResponse>(responseContent);
        }

        public virtual async Task PostAsync(string relativeOrAbsoluteUrl, object request)
        {
            InitializeClient();
            var response = await Client.PostAsJsonAsync(Client.BaseAddress + relativeOrAbsoluteUrl, request);
            response.EnsureSuccessStatusCode();
        }

        public virtual async Task<TResponse> PostAsync<TResponse>(string relativeOrAbsoluteUrl, object request)
        {
            InitializeClient();
            var response = await Client.PostAsJsonAsync(Client.BaseAddress + relativeOrAbsoluteUrl, request);
            response.EnsureSuccessStatusCode();

            // Read the response content as a string
            var responseContent = await response.Content.ReadAsStringAsync();

            // Check if the response content is empty
            if (string.IsNullOrWhiteSpace(responseContent))
            {
                return default(TResponse);
            }

            // Deserialize the response content
            return JsonConvert.DeserializeObject<TResponse>(responseContent);
        }

        public virtual async Task<Stream> PostStream(string relativeOrAbsoluteUrl, object request)
        {
            InitializeClient();
            var jsonContent = JsonContent.Create(request);
            var response = await Client.PostAsync(Client.BaseAddress + relativeOrAbsoluteUrl, jsonContent);
            response.EnsureSuccessStatusCode();

            var stream = await response.Content.ReadAsStreamAsync();
            return stream;
        }


        public virtual async Task PutAsync(string relativeOrAbsoluteUrl, object request)
        {
            InitializeClient();
            var response = await Client.PutAsJsonAsync(Client.BaseAddress + relativeOrAbsoluteUrl, request);
            response.EnsureSuccessStatusCode();
        }

        public virtual async Task<TResponse> PutAsync<TResponse>(string relativeOrAbsoluteUrl, object request)
        {
            InitializeClient();
            var response = await Client.PutAsJsonAsync(Client.BaseAddress + relativeOrAbsoluteUrl, request);
            response.EnsureSuccessStatusCode();

            // Read the response content as a string
            var responseContent = await response.Content.ReadAsStringAsync();

            // Check if the response content is empty
            if (string.IsNullOrWhiteSpace(responseContent))
            {
                return default(TResponse);
            }

            // Deserialize the response content
            return JsonConvert.DeserializeObject<TResponse>(responseContent);
        }

        public virtual async Task DeleteAsync(string relativeOrAbsoluteUrl)
        {
            InitializeClient();
            var response = await Client.DeleteAsync(Client.BaseAddress + relativeOrAbsoluteUrl);
            response.EnsureSuccessStatusCode();
        }

        public virtual async Task<TResponse> DeleteAsync<TResponse>(string relativeOrAbsoluteUrl)
        {
            InitializeClient();
            var response = await Client.DeleteAsync(Client.BaseAddress + relativeOrAbsoluteUrl);
            response.EnsureSuccessStatusCode();
            response.EnsureSuccessStatusCode();

            // Read the response content as a string
            var responseContent = await response.Content.ReadAsStringAsync();

            // Check if the response content is empty
            if (string.IsNullOrWhiteSpace(responseContent))
            {
                return default(TResponse);
            }

            // Deserialize the response content
            return JsonConvert.DeserializeObject<TResponse>(responseContent);
        }

        public HttpClient GetHttpClient()
        {
            return this.Client;
        }

        public virtual void InitializeClient()
        {
            if (this.Client != null) return;
            IsAuthorized = false;
            this.Client = new HttpClient { BaseAddress = new Uri(GetBaseUrl()) };
            if (_httpContextAccessor?.HttpContext == null) return;
            var clientCookieContainer = new CookieContainer();
            var cookies = _httpContextAccessor.HttpContext.Request.Cookies.Where(x => !x.Key.StartsWith("La")).ToList(); //remove LaDesk cookies, at is causes confusion at endpoint accountingapi/account/order/calculate/totals
            if (cookies.Count == 0) return;
            foreach (var cookie in cookies)
            {
                try
                {
                    var newCookie = new Cookie(cookie.Key, cookie.Value, "/", "localhost");
                    clientCookieContainer.Add(newCookie);
                }
                catch (Exception exception)
                {
                    Debug.WriteLine("cookie exception");
                    Debug.WriteLine(exception);
                    return;
                }
            }

            IsAuthorized = true;
            var handler = new HttpClientHandler { CookieContainer = clientCookieContainer };
            this.Client = new HttpClient(handler) { BaseAddress = new Uri(GetBaseUrl()) };
        }

        public virtual async Task TrySignOutClient()
        {
            InitializeClient();
            IsAuthorized = false;
            try
            {
                var url = $"/NxB.Services.App/NxB.LoginApi/login/session";
                await this.Client.DeleteAsync(url);
            }
            catch { }
        }

        private string GetBaseUrl()
        {
            return (_useHttp ? "http" : "https") + "://localhost:19081";
        }
    }
}