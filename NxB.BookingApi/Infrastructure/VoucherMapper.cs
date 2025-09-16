using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AutoMapper;
using Munk.AspNetCore;
using NxB.BookingApi.Models;
using NxB.Domain.Common.Interfaces;
using NxB.Dto.AccountingApi;

namespace NxB.BookingApi.Infrastructure
{
    public class VoucherMapper
    {
        private readonly IMapper _mapper;
        private readonly IAuthorTranslator<AppDbContext> _authorTranslator;
        private readonly AppDbContext _appDbContext;

        public VoucherMapper(IAuthorTranslator<AppDbContext> authorTranslator, AppDbContext appDbContext, IMapper mapper)
        {
            _authorTranslator = authorTranslator;
            _appDbContext = appDbContext;
            _mapper = mapper;
        }

        public List<TDto> Map<TDto, TVoucher>(List<TVoucher> vouchers, List<CreditNote> creditNotes) where TDto : VoucherDto where TVoucher : Voucher
        {
            var voucherDtos = vouchers.Select(x => _mapper.Map<TDto>(x)).ToList();
            voucherDtos.ForEach(x => x.CreateAuthorName = _authorTranslator.GetName(x.CreateAuthorId, _appDbContext));

            PopulateIsCreditedOnDtos(voucherDtos, creditNotes ?? new List<CreditNote>());

            return voucherDtos;
        }

        private void PopulateIsCreditedOnDtos<TDto>(List<TDto> voucherDtos, List<CreditNote> creditNotes) where TDto : VoucherDto
        {
            foreach (var dto in voucherDtos)
            {
                var propertyInfo = dto.GetType().GetProperty("IsCredited");
                if (propertyInfo != null)
                {
                    propertyInfo.SetValue(dto, creditNotes.Any(x => x.InvoiceId == dto.Id));
                }
            }
        }

        public TDto Map<TDto, TVoucher>(TVoucher voucher, List<CreditNote> creditNotes) where TDto : VoucherDto where TVoucher : Voucher
        {
            if (voucher == null) return null;
            var dto = this.Map<TDto, TVoucher>(new List<TVoucher> { voucher }, creditNotes).First();
            return dto;
        }
    }
}
