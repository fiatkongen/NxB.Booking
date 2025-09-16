using System;
using System.Collections.Generic;
using System.Text;

namespace NxB.Dto.AccountingApi
{
    public class CreateLicensePlateEntryDto
    {
        public LicensePlateType LicensePlateType { get; set; }
        public string Number { get; set; }
    }

    public class LicensePlateEntryDto : CreateLicensePlateEntryDto
    {
        public Guid Id { get; set; }
        public bool IsDeleted { get; set; }
    }
}
