using NxB.Dto.AutomationApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NxB.Clients.Interfaces
{
    public interface ILicensePlateAutomationClient : IAuthorizeClient
    {
        Task<LicensePlateAccessDto?> FindAccess(string id);
        Task<LicensePlateAccessDto> CreateLicensePlateAccess(LicensePlateAccessDto createDto, bool queue = false);
        Task<LicensePlateAccessDto> ModifyLicensePlateAccess(LicensePlateAccessDto modifyDto, bool queue = false);
        Task<LicensePlateAccessDto> ModifyLicensePlateAccessInterval(ModifyLicensePlateAccessIntervalDto modifyDto, bool queue = false);
        Task DeleteLicensePlateAccess(string id);
    }
}
