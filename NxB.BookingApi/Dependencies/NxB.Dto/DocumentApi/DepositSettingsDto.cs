using System;
using System.Collections.Generic;
using System.Text;

namespace NxB.Dto.DocumentApi
{
    public class DepositSettingsDto
    {
        public decimal? DepositPercent { get; set; }
        public decimal? DepositMin { get; set; }
        public decimal? DepositMax { get; set; }
    }
}
