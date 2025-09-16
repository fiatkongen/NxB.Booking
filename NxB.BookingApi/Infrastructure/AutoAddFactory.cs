using System;
using System.Collections.Generic;
using System.Text;
using AutoMapper;
using NxB.Dto.OrderingApi;
using NxB.BookingApi.Models;

namespace NxB.BookingApi.Infrastructure
{
    public class AutoAddFactory
    {
        private readonly IMapper _mapper;

        public AutoAddFactory(IMapper mapper)
        {
            _mapper = mapper;
        }

        public AutoAdd Create(CreateAutoAddDto dto)
        {
            var newAutoAdd = new AutoAdd(Guid.NewGuid());
            _mapper.Map(dto, newAutoAdd);
            return newAutoAdd;
        }
    }
}
