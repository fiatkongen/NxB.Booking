using System;
using System.Collections.Generic;
using System.Text;
using AutoMapper;
using NxB.Allocating.Shared.Model;
using NxB.Dto.AllocationApi;

namespace NxB.Allocating.Shared.Infrastructure
{
    public class AllocationMapper
    {
        private readonly IMapper _mapper = ConfigureMapper();

        public AllocationDto Map(Allocation allocation)
        {
            var dto = _mapper.Map<AllocationDto>(allocation);
            return dto;
        }

        private static IMapper ConfigureMapper()
        {
            var mapper = new MapperConfiguration(cfg =>
                {
                    cfg.CreateMap<AllocationDto, Allocation>();
                    cfg.CreateMap<Allocation, AllocationDto>()
                        .ForMember(dest => dest.Duration, options => options.MapFrom(src => src.Duration.Days));
                })
                .CreateMapper();

            return mapper;
        }
    }
}
