using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AutoMapper;
using NxB.Dto.TallyWebIntegrationApi;
using NxB.BookingApi.Models;

namespace NxB.BookingApi.Infrastructure
{
    public class TallyMappingProfile : Profile
    {
        public TallyMappingProfile()
        {
            CreateMap<CreateTallyMasterRadioTenantMapDto, TConMasterRadioTenantMap>();
            CreateMap<TConMasterRadioTenantMap, TallyMasterRadioTenantMapDto>();

            CreateMap<ISocketBase, SocketDto>();
            CreateMap<SocketTBB, SocketDto>();
            CreateMap<SocketTWC, SocketDto>();
            CreateMap<SocketTWEV, ChargerDto>();

            CreateMap<RadioBase, RadioDto>();
            CreateMap<TConTWCConsumption, SocketConsumption>()
                .ForMember(x => x.Id, options => options.MapFrom(src => src.Idx))
                .ForMember(x => x.RadioAddress, options => options.MapFrom(src => src.RadioAddr))
                .ForMember(x => x.IsOpenedByKeyCode, options => options.MapFrom(src => src.OpenByKeyCode));

            CreateMap<TConTBBConsumption, SocketConsumption>()
                .ForMember(x => x.Id, options => options.MapFrom(src => src.Idx))
                .ForMember(x => x.RadioAddress, options => options.MapFrom(src => src.RadioAddr));
            CreateMap<SocketConsumption, SocketConsumptionDto>();

            CreateMap<TConTWEVConsumption, ChargeConsumption>()
                .ForMember(x => x.Id, options => options.MapFrom(src => src.Idx))
                .ForMember(x => x.RadioAddress, options => options.MapFrom(src => src.RadioAddr));
            CreateMap<ChargeConsumption, ChargeConsumptionDto>();

            CreateMap<RadioAccessCode, RadioAccessCodeDto>()
                .ForMember(x => x.Code, options => options.MapFrom(src => (uint)src.Code));
            CreateMap<MasterRadio, MasterRadioDto>();

            CreateMap<Switch, SwitchDto>();
            CreateMap<SwitchLog, SwitchLogDto>()
                .ForMember(x => x.OpenByCode, options => options.MapFrom(src => (uint)src.OpenByCode));

            CreateMap<CreateAccessGroupDto, AccessGroup>();
            CreateMap<ModifyAccessGroupDto, AccessGroup>();
            CreateMap<AccessGroup, AccessGroupDto>();

            CreateMap<SetupPeriod, SetupPeriodDto>().ReverseMap();
            CreateMap<SetupAccess, SetupAccessDto>().ReverseMap();

            CreateMap<RadioBilling, RadioBillingDto>().ReverseMap();

            CreateMap<RadioBilling, RadioBillingDto>().ReverseMap();

            CreateMap<RadioAccessCodeTenantDto, CreateRadioAccessFromAccessibleItemsDto>()
                .ForMember(x => x.Code, options => options.MapFrom(src => (uint)src.Code));

            CreateMap<SocketSwitchController, SocketSwitchControllerDto>();
        }
    }
}