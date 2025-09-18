using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NxB.Domain.Common.Enums;
using NxB.BookingApi.Models;

namespace NxB.BookingApi.Infrastructure
{
    public class TConSqlBuilder : ITConSqlBuilder
    {
        private readonly ITConService _tConService;

        public TConSqlBuilder(ITConService tConService)
        {
            _tConService = tConService;
        }

        public async Task<string> BuildWhereClauseTBDRadios()
        {
            var tbdRadios = await _tConService.FindAllTBDRadios();
            if (tbdRadios.Count == 0) return " 1=0 ";
            var whereClauseRadios = string.Join(" OR ", tbdRadios.Select(x => " Radio.RadioAddr=" + x.RadioAddress));
            return whereClauseRadios;
        }

        public async Task<string> BuildWhereClauseTBERadios()
        {
            var tbeRadios = await _tConService.FindAllTBERadios();
            if (tbeRadios.Count == 0) return " 1=0 ";
            var whereClauseRadios = string.Join(" OR ", tbeRadios.Select(x => " Radio.RadioAddr=" + x.RadioAddress));
            return whereClauseRadios;
        }

        public async Task<string> BuildWhereClauseRadios()
        {
            var radios = await _tConService.FindAllTBDRadios();
            if (radios.Count == 0) return " 1=0 ";
            var whereClauseRadios = string.Join(" OR ", radios.Select(x => " Radio.RadioAddr=" + x.RadioAddress));
            return whereClauseRadios;
        }

        public string BuildWhereClauseRadioCodes(List<int> codes, TConRadioType radioType)
        {
            if (codes.Count == 0) return "";
            string columnName = null;

            switch (radioType)
            {
                case TConRadioType.TBE:
                    columnName = "TBE8consumption.OpenByCode";
                    break;
                case TConRadioType.TBD:
                    columnName = "TBD1accessLog.Code";
                    break;
                default:
                    return "";
            }
            var whereClauseRadioCodes = " AND (" + string.Join(" OR ", codes.Select(x => $" {columnName}=" + x)) + ")";
            return whereClauseRadioCodes;
        }
    }
}