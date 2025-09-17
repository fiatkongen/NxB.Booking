using NxB.Dto.Clients;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using NxB.Dto.AutomationApi;
using NxB.Dto.AllocationApi;
using ServiceStack;

namespace NxB.BookingApi.Infrastructure
{
    public class LicensePlateAutomationClient : NxBAdministratorClient, ILicensePlateAutomationClient
    {
        public static string SERVICEURL = "/NxB.Services.App/NxB.AutomationApi";

        public LicensePlateAutomationClient(IHttpContextAccessor httpContextAccessor) : base(httpContextAccessor)
        {
        }

        public async Task<LicensePlateAccessDto> CreateLicensePlateAccess(LicensePlateAccessDto createDto, bool queue = false)
        {
            var url = $"{SERVICEURL}/licenseplateautomation" + (queue ? "?queue=true" : "?queue=false");
            return await this.PostAsync<LicensePlateAccessDto>(url, createDto);
        }

        public async Task<LicensePlateAccessDto> ModifyLicensePlateAccess(LicensePlateAccessDto modifyDto, bool queue = false)
        {
            var url = $"{SERVICEURL}/licenseplateautomation" + (queue ? "?queue=true" : "?queue=false");
            return await this.PutAsync<LicensePlateAccessDto>(url, modifyDto);
        }

        public async Task<LicensePlateAccessDto> ModifyLicensePlateAccessInterval(ModifyLicensePlateAccessIntervalDto modifyDto, bool queue = false)
        {
            var url = $"{SERVICEURL}/licenseplateautomation/interval" + (queue ? "?queue=true" : "?queue=false");
            return await this.PostAsync<LicensePlateAccessDto>(url, modifyDto);
        }

        public async Task DeleteLicensePlateAccess(string id)
        {
            var url = $"{SERVICEURL}/licenseplateautomation/{id}";
            await this.DeleteAsync(url);
        }

        public async Task<LicensePlateAccessDto?> FindAccess(string id)
        {
            var url = $"{SERVICEURL}/licenseplateautomation/{id}";
            return await this.GetAsync<LicensePlateAccessDto>(url);
        }

        public async Task OpenVirtualGateIn(string licensePlate){
            var url = $"{SERVICEURL}/licenseplateautomation/openvirtualgatein?licensePlate={licensePlate}";
            await this.PostAsync(url, null);
        }
    }
}
