using System;
using System.Collections.Generic;
using System.Text;
using AutoMapper;
using NxB.Domain.Common.Enums;
using NxB.Domain.Common.Interfaces;
using NxB.Dto.OrderingApi;
using NxB.BookingApi.Models;

namespace NxB.BookingApi.Infrastructure
{
    public class AllocationStateMapper
    {
        private readonly IMapper _mapper;
        private readonly IClaimsProvider _claimsProvider;

        public AllocationStateMapper(IClaimsProvider claimsProvider, IMapper mapper)
        {
            _claimsProvider = claimsProvider;
            _mapper = mapper;
        }

        public AllocationStateDto Map(AllocationState model)
        {
            var dto = _mapper.Map<AllocationStateDto>(model);
            return dto;
        }

        public void MapArrival(AllocationState model, AddAllocationStateDto addDto)
        {
            model.AddArrivalLog(_claimsProvider.GetUserId(), addDto.Status.HasValue ? (ArrivalStatus?)addDto.Status.Value : null, addDto.CustomTime, addDto.Text);
        }

        public void MapDeparture(AllocationState model, AddAllocationStateDto addDto)
        {
            model.AddDepartureLog(_claimsProvider.GetUserId(), addDto.Status.HasValue ? (DepartureStatus?) addDto.Status.Value : null, addDto.CustomTime, addDto.Text);
        }
    }
}
