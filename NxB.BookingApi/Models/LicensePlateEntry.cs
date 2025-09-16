using System;
using System.Collections.Generic;
using System.Text;
using NxB.Dto.AccountingApi;

namespace NxB.BookingApi.Models
{
    [Serializable]
    public class LicensePlateEntry
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public int Index { get; set; }
        public LicensePlateType LicensePlateType { get; set; }
        public string Number { get; set; }
        public bool IsDeleted { get; set; }

        private LicensePlateEntry() { }

        public LicensePlateEntry(LicensePlateType licensePlateType, string number)
        {
            LicensePlateType = licensePlateType;
            Number = number;
        }
    }
}
