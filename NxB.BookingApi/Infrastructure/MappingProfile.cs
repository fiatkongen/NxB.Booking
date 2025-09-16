using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using NxB.Dto.LoginApi;
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
        }
    }
}
