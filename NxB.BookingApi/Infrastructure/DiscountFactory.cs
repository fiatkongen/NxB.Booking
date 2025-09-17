using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using AutoMapper;
using NxB.Domain.Common.Interfaces;
using NxB.Dto.OrderingApi;
using NxB.BookingApi.Models;

namespace NxB.BookingApi.Infrastructure
{
    public class DiscountFactory
    {
        private readonly IMapper _mapper = ConfigureMapper();
        private readonly IClaimsProvider _claimsProvider;

        public DiscountFactory(IClaimsProvider claimsProvider)
        {
            _claimsProvider = claimsProvider;
        }

        public DiscountDto Map(Discount model)
        {
            var dto = _mapper.Map<DiscountDto>(model);
            return dto;
        }

        public Discount Map(CreateDiscountDto dto)
        {
            var model = _mapper.Map<Discount>(dto);
            model.TenantId = _claimsProvider.GetTenantId();
            return model;
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public Discount Create(CreateDiscountDto dto)
        {
            var tenantId = _claimsProvider.GetTenantId();

            var discount = _mapper.Map<CreateDiscountDto, Discount>(dto);
            discount.Id = Guid.NewGuid();
            discount.TenantId = tenantId;
            return discount;
        }

        private static IMapper ConfigureMapper()
        {
            var mapper = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<CreateDiscountDto, Discount>();
                cfg.CreateMap<Discount, DiscountDto>();
                cfg.CreateMap<DiscountDto, Discount>();

                cfg.CreateMap<DiscountGroupSelectionDto, DiscountGroupSelection>();
                cfg.CreateMap<DiscountGroupSelection, DiscountGroupSelectionDto>();
            }).CreateMapper();

            return mapper;
        }
    }
}