using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NxB.Allocating.Shared.Model;
using NxB.Domain.Common.Constants;
using NxB.Domain.Common.Dto;
using NxB.Dto.AccountingApi;
using NxB.Dto.OrderingApi;
using ServiceStack;

namespace NxB.BookingApi.Infrastructure
{
    public class CartDtoToCreateMapper
    {
        public CreateOrderDto MapOrder(CartDto cartDto, Guid accountId)
        {
            var createOrderDto = new CreateOrderDto();
            createOrderDto.AccountId = accountId;
            createOrderDto.Note = string.IsNullOrWhiteSpace(cartDto.Note) ? null : cartDto.Note;

            createOrderDto.SubOrders = cartDto.BookingCartItems.Select(x => new CreateSubOrderDto
            {
                AllocationOrderLines = new List<CreateAllocationOrderLineDto>
                {
                    new()
                    {
                        Start = cartDto.Start,
                        End = cartDto.End,
                        Number = 1,
                        ResourceId = x.RentalUnitId.Value,
                        Text = x.RentalUnitName + (x.RentalSubTypeName != null ? $" ({x.RentalSubTypeName})" : ""),
                        SuggestedPricePcs = x.PriceValidate,
                        PriceProfileId = x.PriceProfileId,
                        PriceProfileName = x.PriceProfileName,
                        IsCustomPricePcs = x.IsCustomPricePcs,
                        RentalSubTypeId = x.RentalSubTypeId,
                    }
                },
                GuestOrderLines = x.GuestUnitCartItems.Select(g =>
                    new CreateGuestOrderLineDto
                    {
                        Start = cartDto.Start,
                        End = cartDto.End,
                        Number = g.Number,
                        ResourceId = g.Id,
                        Text = g.TypeName,
                        SuggestedPricePcs = g.PriceValidate,
                        PriceProfileId = g.PriceProfileId,
                        PriceProfileName = g.PriceProfileName,
                        IsCustomPricePcs = x.IsCustomPricePcs
                    }).ToList(),
                ArticleOrderLines = x.ArticleUnitCartItems.Select(a =>
                    new CreateArticleOrderLineDto
                    {
                        Number = a.Number,
                        ResourceId = a.Id,
                        Text = a.TypeName,
                        SuggestedPricePcs = a.PriceValidate,
                        PriceProfileId = a.PriceProfileId,
                        PriceProfileName = a.PriceProfileName,
                        IsCustomPricePcs = x.IsCustomPricePcs

                    }).ToList(),
            }).ToList();

            return createOrderDto;
        }

        public CreateCustomerDto MapCustomer(CartDto cartDto)
        {
            var createCustomerDto = new CreateCustomerDto();
            var customer = cartDto.Customer;
            createCustomerDto.Address = new AddressDto
            {
                Zip = customer.Zip,
                City = customer.City,
                CountryId = customer.CountryId,
                Street = customer.Address
            };

            createCustomerDto.Fullname = new NameDto { Firstname = customer.Firstname, Lastname = customer.Lastname };

            if (!string.IsNullOrEmpty(customer.LicensePlate))
            {
                createCustomerDto.NewLicensePlateEntries = new List<CreateLicensePlateEntryDto>
                {
                    new() { LicensePlateType = LicensePlateType.Car, Number = customer.LicensePlate }
                };
            }

            createCustomerDto.NewPhoneEntries = new List<CreatePhoneEntryDto>
            {
                new()
                {
                    ContactPriority = ContactPriority.Primary, Number = customer.Phone, Prefix = customer.Prefix,
                    SuggestForSms = true
                }
            };

            createCustomerDto.NewAccounts = new List<CreateCustomerAccountDto> { new() { Name = AppConstants.DEFAULT_ACCOUNT_NAME } };
            createCustomerDto.NewEmailEntries = new List<CreateEmailEntryDto> { new() { ContactPriority = ContactPriority.Primary, Email = customer.Email, SuggestForEmails = true } };

            return createCustomerDto;
        }
    }
}
