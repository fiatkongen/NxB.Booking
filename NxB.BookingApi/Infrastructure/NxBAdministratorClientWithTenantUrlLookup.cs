using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Munk.AspNetCore;
using NxB.Domain.Common.Constants;
using NxB.Domain.Common.Interfaces;
using NxB.Domain.Common.Model;
using NxB.Dto.Exceptions;
using NxB.Dto.OrderingApi;
using ServiceStack;

namespace NxB.BookingApi.Infrastructure
{
    public class NxBAdministratorClientWithTenantUrlLookup : NxBClient
    {
        protected Guid? _tenantId;

        public NxBAdministratorClientWithTenantUrlLookup(IHttpContextAccessor httpContextAccessor) : base(httpContextAccessor)
        {
            this._tenantId = httpContextAccessor?.HttpContext != null ? new ClaimsProvider(httpContextAccessor).GetTenantIdOrDefault() : null;
        }

        public override async Task GetAsync(string relativeOrAbsoluteUrl)
        {
            await ParseQueryAndAuthorize(relativeOrAbsoluteUrl);
            await base.GetAsync(relativeOrAbsoluteUrl);
        }

        public override async Task<TResponse> GetAsync<TResponse>(string relativeOrAbsoluteUrl)
        {
            await ParseQueryAndAuthorize(relativeOrAbsoluteUrl);
            return await base.GetAsync<TResponse>(relativeOrAbsoluteUrl);
        }

        public override async Task PostAsync(string relativeOrAbsoluteUrl, object request)
        {
            await ParseQueryAndAuthorize(relativeOrAbsoluteUrl);
            await base.PostAsync(relativeOrAbsoluteUrl, request);
        }

        public override async Task<TResponse> PostAsync<TResponse>(string relativeOrAbsoluteUrl, object request)
        {
            await ParseQueryAndAuthorize(relativeOrAbsoluteUrl);
            return await base.PostAsync<TResponse>(relativeOrAbsoluteUrl, request);
        }

        public override async Task PutAsync(string relativeOrAbsoluteUrl, object request)
        {
            await ParseQueryAndAuthorize(relativeOrAbsoluteUrl);
            await base.PutAsync(relativeOrAbsoluteUrl, request);
        }

        public override async Task<TResponse> PutAsync<TResponse>(string relativeOrAbsoluteUrl, object request)
        {
            await ParseQueryAndAuthorize(relativeOrAbsoluteUrl);
            return await base.PutAsync<TResponse>(relativeOrAbsoluteUrl, request);
        }

        public override async Task DeleteAsync(string relativeOrAbsoluteUrl)
        {
            await ParseQueryAndAuthorize(relativeOrAbsoluteUrl);
            await base.DeleteAsync(relativeOrAbsoluteUrl);
        }

        public override async Task<TResponse> DeleteAsync<TResponse>(string relativeOrAbsoluteUrl)
        {
            await ParseQueryAndAuthorize(relativeOrAbsoluteUrl);
            return await base.DeleteAsync<TResponse>(relativeOrAbsoluteUrl);
        }

        private async Task AuthorizeOnException<TResponse>(WebServiceException exception)
        {
            if (exception != null && exception.ErrorCode != "Unauthorized") throw exception;
            IsAuthorized = false;
            await AuthorizeClient();
        }

        //do not use, but have to
        protected virtual async Task ParseQueryAndAuthorize(string relativeOrAbsoluteUrl)
        {
            var startIndex = relativeOrAbsoluteUrl.IndexOf('?');
            Guid? tenantId = null;
            if (startIndex > -1)
            {
                var queryDictionary =
                    Microsoft.AspNetCore.WebUtilities.QueryHelpers.ParseQuery(
                        relativeOrAbsoluteUrl.Substring(startIndex));
                var tenantIdString = queryDictionary.FirstOrDefault(x => x.Key == "tenantId").Value.FirstOrDefault();
                if (tenantIdString != null)
                {
                    tenantId = Guid.Parse(tenantIdString);
                }
            }

            if (tenantId != null)
            {
                await AuthorizeClient(tenantId);
            }
        }

        public override Task TrySignOutClient()
        {
            this._tenantId = null;
            return base.TrySignOutClient();
        }

        public async Task AuthorizeClient(Guid? tenantId = null)
        {
            var tenantIdOrDefault = this._tenantId;
            var useTenantId = tenantId ?? tenantIdOrDefault;

            if (useTenantId == null && !IsAuthorized)
            {
                return; // Maybe Auth is not needed (AllowAnon...)
            }

            if (useTenantId == null && IsAuthorized)
            {
                await TrySignOutClient();
                return;
            }

            if (IsAuthorized && useTenantId == tenantIdOrDefault) return;
            if (IsAuthorized && useTenantId != tenantIdOrDefault) await TrySignOutClient();
            var url =
                $"/NxB.Services.App/NxB.LoginApi/login/session/json?login=Administrator&password=adm123larras&clientId={useTenantId}";
            await this.GetAsync(url);
            IsAuthorized = true;
            this._tenantId = useTenantId;
        }
        
        public async Task<T> Call<T>(Func<Task<T>> action)
        {
            try
            {
                return await action();
            }
            catch (WebServiceException exception)
            {
                if (exception.ErrorCode != "Unauthorized") throw;
                await AuthorizeClient();
                return await action();
            }
        }

        public async Task Call(Func<Task> action)
        {
            try
            {
                await action();
            }
            catch (WebServiceException exception)
            {
                if (exception.ErrorCode != "Unauthorized") throw;
                await AuthorizeClient();
                await action();
            }
        }
    }

    public class NxBAdministratorClient : NxBAdministratorClientWithTenantUrlLookup
    {
        public NxBAdministratorClient(IHttpContextAccessor httpContextAccessor) : base(httpContextAccessor)
        {
        }

        protected override async Task ParseQueryAndAuthorize(string relativeOrAbsoluteUrl)
        {
            // do nothing
        }
    }
}
