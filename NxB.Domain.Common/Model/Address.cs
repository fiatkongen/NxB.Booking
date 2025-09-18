using System;
using Munk.Utils.Object;

namespace NxB.Domain.Common.Model
{
    [Serializable]
    public class Address : ValueObject<Address>
    {
        public string Street { get; private set; }
        public string Zip { get; private set; }
        public string City { get; private set; }
        public string CountryId { get; private set; }

        private Address() { }
        public Address(string street, string zip, string city, string countryId)
        {
            Street = street;
            Zip = zip;
            City = city;
            CountryId = countryId;
        }
    }
}