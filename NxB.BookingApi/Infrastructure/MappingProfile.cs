using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using NxB.Dto.LoginApi;
using NxB.Dto.TenantApi;
using NxB.BookingApi.Models;

namespace NxB.BookingApi.Infrastructure
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<UserDto, User>()
                .ForMember(x => x.UserTenantAccesses, opt => opt.Ignore())
                .ForMember(x => x.Password, opt => opt.Ignore())
                .ForMember(x => x.Roles, options => options.MapFrom(src => string.Join(',', src.Roles)));

            CreateMap<User, UserDto>()
                .ForMember(x => x.Roles, options => options.MapFrom(src => src.Roles != null ? src.Roles.Split(',', StringSplitOptions.None).ToList() : new List<string>()));

            CreateMap<CreateUserDto, User>()
                .ForMember(x => x.UserTenantAccesses, opt => opt.Ignore())
                .ForMember(x => x.Roles, options => options.MapFrom(src => string.Join(',', src.Roles)));

            // Tenant API mappings
            CreateMap<TenantDto, Tenant>()
                .DisableCtorValidation()
                .ForMember(x => x.Id, opt => opt.Ignore());

            CreateMap<Tenant, TenantDto>();
            CreateMap<Tenant, TenantPublicDto>();
            CreateMap<ModifyTenantPublicDto, Tenant>();
            CreateMap<CreateTenantDto, Tenant>();
            CreateMap<TenantDto, TenantPublicDto>().ReverseMap();

            CreateMap<CreateTextSectionDto, TextSection>();
            CreateMap<TextSection, TextSectionDto>().ReverseMap();
            CreateMap<ModifyTextSectionDto, TextSection>();

            CreateMap<CreateBillableItemDto, BillableItem>();
            CreateMap<BillableItem, BillableItemDto>();

            CreateMap<CreateKioskDto, Kiosk>();
            CreateMap<ModifyKioskDto, Kiosk>();
            CreateMap<Kiosk, KioskDto>().ForMember(dest => dest.LastState, options => options.MapFrom(src => src.LastOnline.HasValue && src.LastOnline < DateTime.Now.ToEuTimeZone().AddMinutes(-5) ? KioskState.Offline : src.LastState));

            CreateMap<CreateFeatureModuleDto, FeatureModule>();
            CreateMap<ModifyFeatureModuleDto, FeatureModule>();
            CreateMap<FeatureModule, FeatureModuleDto>();

            CreateMap<FeatureModuleTenantEntry, FeatureModuleTenantEntryDto>();
            CreateMap<ModifyAdminFeatureModuleTenantEntryDto, FeatureModuleTenantEntry>();

            CreateMap<ExternalPaymentTransaction, ExternalPaymentTransactionDto>();
        }
    }
}
