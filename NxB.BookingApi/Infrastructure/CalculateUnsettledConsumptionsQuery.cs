using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Munk.AspNetCore.Sql;
using Newtonsoft.Json;
using NxB.Domain.Common.Enums;
using NxB.Dto.TallyWebIntegrationApi;
using NxB.BookingApi.Models;
using ServiceStack;

namespace NxB.BookingApi.Infrastructure
{
    public class CalculateUnsettledConsumptionsQuery
    {
        private readonly TallyDbConnectionFactory _tallyDbConnectionFactory;
        private readonly ITConSqlBuilder _tConSqlBuilder;

        public CalculateUnsettledConsumptionsQuery(TallyDbConnectionFactory tallyDbConnectionFactory, ITConSqlBuilder tConSqlBuilder)
        {
            _tallyDbConnectionFactory = tallyDbConnectionFactory;
            _tConSqlBuilder = tConSqlBuilder;
        }

        public async Task<List<ConsumptionTotalDto>> ExecuteAsync(List<int> codes, DateTime? filterFrom = null)
        {
            List<ConsumptionTotalDto> consumptionTotalDtos = new List<ConsumptionTotalDto>();

            await using var context = _tallyDbConnectionFactory.Create();
            var commandText = $@"SELECT Idx, [DateTime] as 'CreateDate', [SavedDateTime] as 'CreateDateDB', TBD1accessLog.RadioAddr as 'RadioAddress', TBD1accessLog.KeyCode as 'IsKeyCode', TBD1accessLog.Code, TBD1status._Name AS 'RadioName', TBD1accessLog.PulseTime AS 'PulseTime', 1 as 'Consumed'
                      FROM [TCon].[dbo].[TBD1accessLog]
                      INNER JOIN Radio ON Radio.RadioAddr = TBD1accessLog.RadioAddr
                        {_tConSqlBuilder.BuildWhereClauseRadioCodes(codes, TConRadioType.TBD)} AND TBD1accessLog.Rejected = 0 AND TBD1accessLog.__Recorded = 0
                        {(filterFrom.HasValue ? $@" AND [DateTime] > '{filterFrom.Value.ToSql()}'" : "")}
                        AND ({await _tConSqlBuilder.BuildWhereClauseTBDRadios()})

                    INNER JOIN TBD1status ON TBD1status.RadioAddr = Radio.RadioAddr

                    UNION ALL

                    SELECT Idx, [CreateDateTime] as 'CreateDate', [CreateDateTime] as 'CreateDateDB', TBE8consumption.RadioAddr as 'RadioAddress', TBE8consumption.OpenByKeyCode as 'IsKeyCode', TBE8consumption.OpenByCode, CAST(SocketNo AS nvarchar(1)) AS 'RadioName', 0 AS 'PulseTime', CAST(TBE8consumption.ConsumptionEnd-TBE8consumption.ConsumptionStart AS decimal(18,2)) AS 'Consumed'
                    FROM [TCon].[dbo].[TBE8consumption]
                    INNER JOIN Radio ON Radio.RadioAddr = TBE8consumption.RadioAddr AND TBE8consumption.__Recorded = 0
                    {(filterFrom.HasValue ? $@" AND [CreateDateTime] > '{filterFrom.Value.ToSql()}'" : "")}
                    AND ({await _tConSqlBuilder.BuildWhereClauseTBERadios()})
                    {_tConSqlBuilder.BuildWhereClauseRadioCodes(codes, TConRadioType.TBE)}

                    ORDER BY  CreateDate DESC
                    ";

            var cmd = new SqlCommand
            {
                CommandText =
                    commandText,
                CommandType = CommandType.Text,
                Connection = context
            };

            context.Open();

            var reader = await cmd.ExecuteReaderAsync();

            while (reader.Read())
            {
                ConsumptionTotalDto consumptionTotalDto = new ConsumptionTotalDto
                {
                    Idx = reader.GetInt32(reader.GetOrdinal("Idx")),
                    CreateDate = reader.GetDateTime(reader.GetOrdinal("CreateDate")),
                    CreateDateDB = reader.GetDateTime(reader.GetOrdinal("CreateDateDB")),
                    RadioAddress = reader.GetInt32(reader.GetOrdinal("RadioAddress")),
                    RadioName = reader.GetString(reader.GetOrdinal("RadioName")),
                    IsKeyCode = reader.GetBoolean(reader.GetOrdinal("IsKeyCode")),
                    Code = (uint)reader.GetInt32(reader.GetOrdinal("Code")),
                    PulseTime = reader.GetInt32(reader.GetOrdinal("PulseTime")),
                    Consumed = reader.GetDecimal(reader.GetOrdinal("Consumed")),
                };
                consumptionTotalDtos.Add(consumptionTotalDto);;
            }

            reader.Close();

            return consumptionTotalDtos;
        }
    }
}