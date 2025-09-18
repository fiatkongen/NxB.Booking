using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Munk.AspNetCore.Sql;
using Newtonsoft.Json;
using NxB.Dto.TallyWebIntegrationApi;
using NxB.BookingApi.Models;
using ServiceStack;

namespace NxB.BookingApi.Infrastructure
{
    public class CalculateConsumptionsQuery
    {
        private readonly TallyDbConnectionFactory _tallyDbConnectionFactory;
        private readonly ITConSqlBuilder _tConSqlBuilder;

        public CalculateConsumptionsQuery(TallyDbConnectionFactory tallyDbConnectionFactory, ITConSqlBuilder tConSqlBuilder)
        {
            _tallyDbConnectionFactory = tallyDbConnectionFactory;
            _tConSqlBuilder = tConSqlBuilder;
        }

        public async Task<List<ConsumptionTotalDto>> ExecuteAsync(int? code, DateTime filterFrom, DateTime? filterTo)
        {
            List<ConsumptionTotalDto> consumptionTotalDtos = new List<ConsumptionTotalDto>();

            await using var context = _tallyDbConnectionFactory.Create();
            var commandText = $@"SELECT Idx, [DateTime] as 'CreateDate', TBD1accessLog.RadioAddr as 'RadioAddress', TBD1accessLog.KeyCode as 'IsKeyCode', TBD1accessLog.Code, TBD1status._Name AS 'RadioName', TBD1accessLog.PulseTime AS 'PulseTime'
                      FROM [TCon].[dbo].[TBD1accessLog]
                      INNER JOIN Radio ON Radio.RadioAddr = TBD1accessLog.RadioAddr
                        AND (PulseTime > 15 AND PulseTime < 10000
                            OR Radio.RadioAddr = 100504 -- Kommandørgården
                            OR Radio.RadioAddr = 100637 OR Radio.RadioAddr = 100639 OR Radio.RadioAddr = 100640 OR Radio.RadioAddr = 100667 --Byaasgaard
                            ) AND Rejected = 0
                        AND [DateTime] > '{filterFrom.ToSql()}'
                        {(filterTo != null ? "AND [DateTime] < '" + filterTo.Value.ToSql() + "'" : "")}
                        AND ({await _tConSqlBuilder.BuildWhereClauseTBDRadios()})
                        AND TBD1accessLog.Code = {code}

                    INNER JOIN TBD1status ON TBD1status.RadioAddr = Radio.RadioAddr

                  --  FOR JSON PATH
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
                    RadioAddress = reader.GetInt32(reader.GetOrdinal("RadioAddress")),
                    RadioName = reader.GetString(reader.GetOrdinal("RadioName")),
                    IsKeyCode = reader.GetBoolean(reader.GetOrdinal("IsKeyCode")),
                    Code = (uint)reader.GetInt32(reader.GetOrdinal("Code")),
                    PulseTime = reader.GetInt32(reader.GetOrdinal("PulseTime")),
                };
                consumptionTotalDtos.Add(consumptionTotalDto);;
            }

            reader.Close();

            return consumptionTotalDtos;
        }
    }
}