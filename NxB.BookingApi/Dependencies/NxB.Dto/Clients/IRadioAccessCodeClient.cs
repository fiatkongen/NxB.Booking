using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using NxB.Dto.OrderingApi;
using NxB.Dto.TallyWebIntegrationApi;

namespace NxB.Dto.Clients
{
    public interface IRadioAccessCodeClient
    {
        Task CreateRadioAccessCode(CreateRadioAccessDto createRadioAccessDto);
        Task CreateRadioAccessCodesFromAccessibleItems(CreateOrModifyAccessFromAccessibleItemsDto createAccessFromAccessibleItemsDto);
        Task ModifyRadioAccessCodesFromAccessibleItems(CreateOrModifyAccessFromAccessibleItemsDto accessFromAccessibleItemsDto);
        Task RemoveAccessFromCode(uint code, bool markAsSettled);

        Task<List<int>> DeleteRadioAccessesCodeFromRadioCodes(List<RadioAccessCodeTenantDto> radioAccessCodeTenantDtos);
        Task<List<int>> DeleteRadioAccessesCodeFromRadioCodesIfActivatedInLog(List<RadioAccessCodeTenantDto> radioAccessCodeTenantDtos);
        Task<int> AddRadioAccessesCodeToRadioCodes(List<RadioAccessCodeTenantDto> radioAccessCodeTenantDtos);
        Task<int> CreateAccessFromAccessibleItems(CreateRadioAccessFromAccessibleItemsDto createDto);
    }
}
