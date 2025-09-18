using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Munk.AspNetCore.Sql;
using NxB.Dto.TallyWebIntegrationApi;
using ServiceStack;

namespace NxB.BookingApi.Infrastructure
{
    public class SystemLogQuery
    {
        private readonly TallyDbConnectionFactory _dbConnectionFactory;

        public SystemLogQuery(TallyDbConnectionFactory dbConnectionFactory)
        {
            _dbConnectionFactory = dbConnectionFactory;
        }

        public async Task<List<SystemLogDto>> ExecuteAsync(int masterAddress)
        {
            var systemLogs = new List<SystemLogDto>();
            await using var sqlConnection = _dbConnectionFactory.Create();
            var commandText = $@"
                    SELECT [DateTime]
                          ,[MasterAddr]
                          ,[RadioAddr]
                          ,[Type]
                          ,[Text]
                      FROM [TCon].[dbo].[SystemLog]
                      WHERE MasterAddr = {masterAddress} and( type = 4 or type = 3)
                      ORDER BY DateTime DESC
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


            while (reader.Read())
            {
                systemLogs.Add(new SystemLogDto
                {
                    CreateDate = reader.GetDateTime(reader.GetOrdinal("DateTime")),
                    Type = reader.GetInt32(reader.GetOrdinal("Type")),
                    Text = reader.GetString(reader.GetOrdinal("Text")),
                });
            }

            reader.Close();

            return systemLogs;
        }
    }
}