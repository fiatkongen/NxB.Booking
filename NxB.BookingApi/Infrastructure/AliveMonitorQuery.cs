using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Itenso.TimePeriod;
using Microsoft.Data.SqlClient;
using Munk.AspNetCore.Sql;
using NxB.Dto.TallyWebIntegrationApi;
using ServiceStack;

namespace NxB.BookingApi.Infrastructure
{
    public class AliveMonitorQuery
    {
        private readonly TallyDbConnectionFactory _dbConnectionFactory;

        public AliveMonitorQuery(TallyDbConnectionFactory dbConnectionFactory)
        {
            _dbConnectionFactory = dbConnectionFactory;
        }

        public async Task<AliveMonitorDto> ExecuteAsync()
        {
            var systemLogs = new List<SystemLogDto>();
            await using var sqlConnection = _dbConnectionFactory.Create();
            var commandText = $@"
                    SELECT *
                      FROM [TCon].[dbo].[AliveMonitor]
                    ";

            var cmd = new SqlCommand
            {
                CommandText =
                    commandText,
                CommandType = CommandType.Text,
                Connection = sqlConnection
            };

            sqlConnection.Open();

            var reader = await cmd.ExecuteReaderAsync();


            if (reader.Read())
            {
                return new AliveMonitorDto
                {
                    CreateDate = reader.GetDateTime(reader.GetOrdinal("DateTime")),
                    Count = reader.GetInt32(reader.GetOrdinal("Count1"))
                };
            }

            reader.Close();

            return null;
        }
    }
}