using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace NxB.Dto.DocumentApi
{
    public class OptiScanResult
    {
        public bool Success { get; set; }
        public List<string> Codes { get; set; }

        public string GetCodesString()
        {
            return string.Join(", ", Codes.Select(TranslateCode));
        }

        public string TranslateCode(string code)
        {
            switch (code)
            {
                case "accessperiodforregistrationnumberalreadyexist":
                    return "Periode for nummerplade eksisterer allerede";
                case "accessperiodcreated":
                    return "Fejl ved opret af adgangsperiode";
                case "otheraccessperiodforregistrationnumberalreadyexist":
                    return "Andre adgangsperioder for nummerplade eksisterer allerede";
                case "accessperiodupdatederror":
                    return "Fejl ved opdatering af adgangsperiode";
                case "accessperiodupdated":
                    return "Adgangsperiode opdateret";
                case "accessperioddeleted":
                    return "Adgangsperiode slettet";
                case "accessperioddeletederror":
                    return "Fejl ved slet af adgangsperiode";
                case "accessperiodsfetched":
                    return "Adgangsperiode hentet";
                case "allregistrationsfetched":
                    return "Alle nummerplader hentet";
                case "allregistrationsfetchederror":
                    return "Fejl ved hent af alle nummerplader";
                default:
                    return code;
            }
        }
    }

    public class OptiScanResultAccessPeriods : OptiScanResult
    {
        public List<OptiScanRegistrationPeriod> RegistrationPeriodItems { get; set; }
    }

    public class OptiScanRegistrationPeriod
    {
        public int Id { get; set; }
        public string RegistrationNumber { get; set; }
        public DateTime? UtcPeriodStart { get; set; }
        public DateTime? UtcPeriodEnd { get; set; }
        public string Note { get; set; }
    }

}
