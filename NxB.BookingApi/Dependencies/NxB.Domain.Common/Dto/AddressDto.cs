using System;
using Munk.Utils.Object;

namespace NxB.Domain.Common.Dto
{
    public class AddressDto 
    {
        public string Street { get; set; }
        public string Zip { get; set; }
        public string City { get; set; }
        public string CountryId { get; set; }
    }
}