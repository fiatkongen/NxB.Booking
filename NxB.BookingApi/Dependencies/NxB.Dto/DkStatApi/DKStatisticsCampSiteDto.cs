using System;
using System.Collections.Generic;

namespace NxB.Dto.DkStatApi
{
    public class DkStatisticsCampSiteChangesDto
    {

        public DateTime StartDate { get; set; }

        public string CvrNo { get; set; }
        public string JournalNo { get; set; }
        public int CampingUnits { get; set; }
        public string Remarks { get; set; } = "";
        public string ContactName { get; set; }
        public string ContactPhone { get; set; }
        public string ContactEmail { get; set; }

        public List<DkStatCountryNonPermanentItems> ListNonPermanent = new();
        public List<DkStatCountryPermanentItems> ListPermanent = new();
    }

    public class DkStatCountryItem
    {
        public string CountryCode { get; set; }
        public string CountryName { get; set; }
        public int StayQuantity { get; set; }
        public int GuestsQuantity { get; set; }

        public DkStatCountryItem(string countryCode, string countryName, int stayQuantity, int guestsQuantity)
        {
            CountryCode = countryCode;
            CountryName = countryName;
            StayQuantity = stayQuantity;
            GuestsQuantity = guestsQuantity;
        }
    }

    public class DkStatCountryNonPermanentItems : DkStatCountryItem
    {
        public DkStatCountryNonPermanentItems(string countryCode, string countryName, int stayQuantity, int guestsQuantity) : base(countryCode, countryName, stayQuantity, guestsQuantity)
        {
        }
    }

    public class DkStatCountryPermanentItems : DkStatCountryItem
    {
        public int SpotsCount { get; set; }

        public DkStatCountryPermanentItems(string countryCode, string countryName, int stayQuantity, int guestsQuantity, int spotsCount) : base(countryCode, countryName, stayQuantity, guestsQuantity)
        {
            SpotsCount = spotsCount;
        }
    }
}
