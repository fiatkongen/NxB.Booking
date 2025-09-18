using System;
using System.Collections.Generic;
using System.Text;
using AutoMapper;
using NxB.BookingApi.Models;

namespace NxB.BookingApi.Infrastructure
{
    public class PricingMappingProfile : Profile
    {
        public PricingMappingProfile()
        {
            CreateMap<PriceProfile, PriceProfileDetails>();
            CreateMap<CreatePriceProfileDetails, PriceProfile>();

            CreateMap<ModifyPriceProfileStatistics, PriceProfile>();

            CreateMap<CostInterval, CostIntervalDetails>();
            CreateMap<CreateCostIntervalDetails, CostInterval>();
        }
    }
}