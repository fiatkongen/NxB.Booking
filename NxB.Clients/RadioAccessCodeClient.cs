using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using NxB.Clients.Interfaces;
using NxB.Dto.OrderingApi;
using NxB.Dto.TallyWebIntegrationApi;
using ServiceStack;

namespace NxB.Clients
{
    public class RadioAccessCodeClient : NxBAdministratorClient, IRadioAccessCodeClient
    {
        public RadioAccessCodeClient(IHttpContextAccessor httpContextAccessor) : base(httpContextAccessor)
        {
        }

        public async Task CreateRadioAccessCode(CreateRadioAccessDto createRadioAccessDto)
        {
            var url = $"/NxB.Services.App/NxB.TallyWebIntegrationApi/radioaccesscode";
            await this.PostAsync(url, createRadioAccessDto);
        }

        public async Task CreateRadioAccessCodesFromAccessibleItems(CreateOrModifyAccessFromAccessibleItemsDto createAccessFromAccessibleItemsDto)
        {
            var url = $"/NxB.Services.App/NxB.TallyWebIntegrationApi/radioaccesscode/accessibleitems";
            await this.PostAsync(url, createAccessFromAccessibleItemsDto);
        }

        public async Task ModifyRadioAccessCodesFromAccessibleItems(CreateOrModifyAccessFromAccessibleItemsDto modifyAccessFromAccessibleItemsDto)
        {
            var url = $"/NxB.Services.App/NxB.TallyWebIntegrationApi/radioaccesscode/accessibleitems";
            await this.PutAsync(url, modifyAccessFromAccessibleItemsDto);
        }

        public async Task RemoveAccessFromCode(uint code, bool markAsSettled)
        {
            var url = $"/NxB.Services.App/NxB.TallyWebIntegrationApi/radioaccesscode/code?code={code}&markAsSettled={markAsSettled}";
            await this.DeleteAsync(url);
        }

        public async Task<List<int>> DeleteRadioAccessesCodeFromRadioCodes(List<RadioAccessCodeTenantDto> radioAccessCodeTenantDtos)
        {
            var url = $"/NxB.Services.App/NxB.TallyWebIntegrationApi/radioaccesscode/deleteaccesscodesfromradiocodes";
            return await this.PostAsync<List<int>>(url, radioAccessCodeTenantDtos);
        }

        public async Task<List<int>> DeleteRadioAccessesCodeFromRadioCodesIfActivatedInLog(List<RadioAccessCodeTenantDto> radioAccessCodeTenantDtos)
        {
            var url = $"/NxB.Services.App/NxB.TallyWebIntegrationApi/radioaccesscode/deleteaccesscodesfromradiocodes/ifactivatedinlog";
            return await this.PostAsync<List<int>>(url, radioAccessCodeTenantDtos);
        }

        public async Task<int> AddRadioAccessesCodeToRadioCodes(List<RadioAccessCodeTenantDto> radioAccessCodeTenantDtos)
        {
            var url = $"/NxB.Services.App/NxB.TallyWebIntegrationApi/radioaccesscode/addaccesscodestoradiocodes";
            return await this.PostAsync<int>(url, radioAccessCodeTenantDtos);
        }

        public async Task<int> CreateAccessFromAccessibleItems(CreateRadioAccessFromAccessibleItemsDto createDto)
        {
            var url = $"/NxB.Services.App/NxB.TallyWebIntegrationApi/radioaccesscode/accessibleitems";
            return await this.PostAsync<int>(url, createDto);
        }
    }
}
