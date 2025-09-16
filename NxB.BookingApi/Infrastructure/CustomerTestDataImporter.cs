using System;
using System.Collections.Generic;
using NxB.BookingApi.Models;
using NxB.Domain.Common.Dto;
using NxB.Dto.AccountingApi;

namespace NxB.BookingApi.Infrastructure
{
    public class CustomerTestDataImporter : ICustomerTestDataImporter
    {
        private readonly CustomerFactory _customerFactory;
        public string[] FirstNames = { "Allan", "Bent", "Carsten", "Michael", "Finn", "Kurt", "Knud", "Thomas", "Bo", "Ib", "Kurt", "Frede", "Aksel", "Ebbe", "Laust", "Vilum", "Troels", "Ulrik", "Verner", "Dennis", "Karl" };
        public string[] LastNames = { "Jensen", "Nielsen", "Hansen", "Pedersen", "Andersen", "Christensen", "Larsen", "Sørensen", "Rasmussen", "Jørgensen", "Petersen", "Madsen", "Kristensen", "Olsen", "Thomsen", "Christiansen", "Poulsen", "Johansen", "Knudsen", "Møller" };

        private int _streetCount = 0;
        public string[] StreetNames =
        {
            "Lærkevej", "Birkevej", "Vibevej", "Vinkelvej", "Østergade", "Engvej", "Vestergade", "Møllevej", "Kirkevej",
            "Bøgevej", "Industrivej", "Tværvej", "Stationsvej", "Elmevej", "Skovvej", "Nørregade", "Bakkevej", "Søndergade", "Skolevej", "Drosselvej"
        };

        private int _cityCount;
        public List<(string, string)> Cities = new()
        {
            ( "2000", "Frederiksberg"),
            ( "3000", "Helsingør"),
            ( "1301", "København K"),
            ( "3700", "Rønne"),
            ( "4000", "Roskilde"),
            ( "5000", "Odense C"),
            ( "6800", "Varde"),
            ( "7000", "Fredericia"),
            ( "7700", "Thisted"),
            ( "7800", "Skive"),
            ( "8600", "Silkeborg"),
            ( "8000", "Aarhus C"),
            ( "9000", "Aalborg"),
            ( "8900", "Randers"),
        };

        public CustomerTestDataImporter(CustomerFactory customerFactory)
        {
            _customerFactory = customerFactory;
        }

        public List<Customer> BuildCustomersTestData(Guid destinationTenantId)
        {
            var customers = new List<Customer>();

            foreach (var lastName in LastNames)
            {
                foreach (var firstName in FirstNames)
                {
                    var city = GetCity();
                    var customer = _customerFactory.CreateTestCustomer(new CreateCustomerDto
                    {
                        Address = new AddressDto
                        {
                            CountryId = "dk",
                            Street = GetStreet(),
                            Zip = city.Item1,
                            City = city.Item2
                        },
                        Fullname = new NameDto
                        {
                            Firstname = firstName,
                            Lastname = lastName
                        },
                        IsCompany = false,
                        NewEmailEntries = new List<CreateEmailEntryDto>
                        {
                            new()
                            {
                                ContactPriority = ContactPriority.Primary,
                                Email = firstName.ToLower() + "@" + lastName.ToLower() + ".dk",
                                SuggestForEmails = true
                            }
                        },
                        NewPhoneEntries = new List<CreatePhoneEntryDto>
                        {
                            new()
                            {
                                Prefix = "+45",
                                Number = GenerateRandomPhone(),
                                ContactPriority = ContactPriority.Primary,
                                SuggestForSms = true
                            }
                        },
                        Att = new NameDto()
                    }, destinationTenantId).Result;
                    customer.IsImported = true;
                    customers.Add(customer);
                }
            }

            return customers;
        }

        public string GetStreet()
        {
            if (_streetCount == StreetNames.Length)
            {
                _streetCount = 0;
            }
            var streetName = StreetNames[_streetCount++] + " " + (new Random().Next(99) + 1);
            return streetName;
        }

        public (string, string) GetCity()
        {
            if (_cityCount == Cities.Count)
            {
                _cityCount = 0;
            }

            return Cities[_cityCount++];
        }

        public string GenerateRandomPhone()
        {
            var phone = "";

            for (int i = 0; i < 8; i++)
            {
                phone += new Random().Next(9).ToString();
            }

            return phone;
        }
    }
}
