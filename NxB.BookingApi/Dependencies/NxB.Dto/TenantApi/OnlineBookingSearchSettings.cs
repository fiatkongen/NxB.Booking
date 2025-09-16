namespace NxB.Dto.TenantApi
{
    public class OnlineBookingSearchSettings
    {
        public int AddDaysForSearch { get; set; } = 14;
        public bool ShowCheaperAlternatives { get; set; } = true;
        public bool ShowAlternatives { get; set; } = true;
        public bool ShowAlternativesOfDifferentDuration { get; set; } = true;
    }
}
