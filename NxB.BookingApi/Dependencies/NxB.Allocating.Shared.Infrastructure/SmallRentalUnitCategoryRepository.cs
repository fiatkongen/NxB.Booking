using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Munk.AspNetCore.Sql;
using Newtonsoft.Json;
using NxB.Allocating.Shared.Model;
using NxB.Domain.Common.Interfaces;
using ServiceStack;

namespace NxB.Allocating.Shared.Infrastructure
{
    public class SmallRentalUnitCategoryRepository : ISmallRentalUnitCategoryRepository
    {
        private readonly IClaimsProvider _claimsProvider;
        private readonly IDbConnectionFactory _dbConnectionFactory;

        public SmallRentalUnitCategoryRepository(IClaimsProvider claimsProvider,
            IDbConnectionFactory dbConnectionFactory)
        {
            _claimsProvider = claimsProvider;
            _dbConnectionFactory = dbConnectionFactory;
        }

        private async Task<List<SmallRentalUnitCategory>> Find(string commandText)
        {
            var stringBuilder = new StringBuilder();
            await using var context = _dbConnectionFactory.Create();
            
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
                stringBuilder.Append(reader.GetString(0)); // Reads in chunks of ~2K Bytes
            }

            reader.Close();

            string json = stringBuilder.ToString();
            if (json.IsEmpty())
                return new List<SmallRentalUnitCategory>();

            var simpleRentalUnitCategories = JsonConvert.DeserializeObject<List<SimpleRentalUnitCategory>>(json);

            var categories = simpleRentalUnitCategories.GroupBy(x => x.RentalCategoryId, x => x, (key, ru) =>
            {
                var rentalUnitIds = ru.ToList();
                return new SmallRentalUnitCategory(key, rentalUnitIds.Select(x => x.Id).ToList());
            }).ToList();

            return categories;
        }

        private class SimpleRentalUnitCategory
        {
            public Guid Id { get; set; }
            public Guid RentalCategoryId { get; set; }
        }

        public Task<List<SmallRentalUnitCategory>> Find()
        {
            var tenantId = _claimsProvider.GetTenantId();
            var commandText = $@"SELECT  
	                Id,
                    RentalCategoryId
                    FROM [RentalUnit] WHERE RentalUnit.IsDeleted=0 AND TenantId = '{tenantId}' 
                    FOR JSON PATH";
            return this.Find(commandText);
        }

        public Task<List<SmallRentalUnitCategory>> FindOnline(Guid? filterRentalCategoryId)
        {
            var tenantId = _claimsProvider.GetTenantId();
            var commandText = $@"SELECT  
	                RentalUnit.Id,
                    RentalUnit.RentalCategoryId
                    FROM [RentalUnit] INNER JOIN RentalCategory ON RentalUnit.RentalCategoryId=RentalCategory.Id AND RentalCategory.IsAvailableOnline=1 {(filterRentalCategoryId != null ? $" AND RentalCategory.Id='{filterRentalCategoryId.Value}'" : "")} AND RentalCategory.IsDeleted=0 AND RentalUnit.IsDeleted=0 AND RentalUnit.IsAvailableOnline=1 AND RentalCategory.TenantId = '{tenantId}' 

                    UNION

					SELECT  
	                cast(cast(0 as binary) as uniqueidentifier),
                    RentalCategory.Id
                    FROM RentalCategory WHERE RentalCategory.IsAvailableOnline=1  AND RentalCategory.IsDeleted=0 AND RentalCategory.TenantId = '{tenantId}' {(filterRentalCategoryId != null ? $" AND RentalCategory.Id='{filterRentalCategoryId.Value}'" : "")} AND (SELECT COUNT(*) FROM RentalUnit WHERE RentalUnit.RentalCategoryId = RentalCategory.Id AND RentalUnit.IsDeleted=0 AND RentalUnit.IsAvailableOnline=1) = 0

                    FOR JSON PATH";
            return this.Find(commandText);
        }

        public Task<List<SmallRentalUnitCategory>> FindKiosk(Guid? filterRentalCategoryId)
        {
            var tenantId = _claimsProvider.GetTenantId();
            var commandText = $@"SELECT  
	                RentalUnit.Id,
                    RentalUnit.RentalCategoryId
                    FROM [RentalUnit] INNER JOIN RentalCategory ON RentalUnit.RentalCategoryId=RentalCategory.Id AND ((RentalCategory.IsAvailableOnline=1 AND RentalCategory.KioskAvailability=0) OR RentalCategory.KioskAvailability=1) {(filterRentalCategoryId != null ? $" AND RentalCategory.Id='{filterRentalCategoryId.Value}'" : "")} AND RentalCategory.IsDeleted=0 AND RentalUnit.IsDeleted=0 AND ((RentalUnit.IsAvailableOnline=1 AND RentalUnit.KioskAvailability=0) OR RentalUnit.KioskAvailability=1) AND RentalCategory.TenantId = '{tenantId}' 

                    UNION

					SELECT  
	                cast(cast(0 as binary) as uniqueidentifier),
                    RentalCategory.Id
                    FROM RentalCategory WHERE ((RentalCategory.IsAvailableOnline=1 AND RentalCategory.KioskAvailability=0) OR RentalCategory.KioskAvailability=1) AND RentalCategory.IsDeleted=0 AND RentalCategory.TenantId = '{tenantId}' {(filterRentalCategoryId != null ? $" AND RentalCategory.Id='{filterRentalCategoryId.Value}'" : "")} AND (SELECT COUNT(*) FROM RentalUnit WHERE RentalUnit.RentalCategoryId = RentalCategory.Id AND RentalUnit.IsDeleted=0 AND RentalUnit.IsAvailableOnline=1) = 0

                    FOR JSON PATH";
            return this.Find(commandText);
        }

        public Task<List<SmallRentalUnitCategory>> FindCtoutvert(Guid? filterRentalCategoryId)
        {
            var tenantId = _claimsProvider.GetTenantId();
            var commandText = $@"SELECT  
	                RentalUnit.Id,
                    RentalUnit.RentalCategoryId
                    FROM [RentalUnit] INNER JOIN RentalCategory ON RentalUnit.RentalCategoryId=RentalCategory.Id AND ((RentalCategory.IsAvailableOnline=1 AND RentalCategory.CtoutvertAvailability=0) OR RentalCategory.CtoutvertAvailability=1) {(filterRentalCategoryId != null ? $" AND RentalCategory.Id='{filterRentalCategoryId.Value}'" : "")} AND RentalCategory.IsDeleted=0 AND RentalUnit.IsDeleted=0 AND ((RentalUnit.IsAvailableOnline=1 AND RentalUnit.CtoutvertAvailability=0) OR RentalUnit.CtoutvertAvailability=1) AND RentalCategory.TenantId = '{tenantId}' 

                    UNION

					SELECT  
	                cast(cast(0 as binary) as uniqueidentifier),
                    RentalCategory.Id
                    FROM RentalCategory WHERE ((RentalCategory.IsAvailableOnline=1 AND RentalCategory.CtoutvertAvailability=0) OR RentalCategory.CtoutvertAvailability=1) AND RentalCategory.IsDeleted=0 AND RentalCategory.TenantId = '{tenantId}' {(filterRentalCategoryId != null ? $" AND RentalCategory.Id='{filterRentalCategoryId.Value}'" : "")} AND (SELECT COUNT(*) FROM RentalUnit WHERE RentalUnit.RentalCategoryId = RentalCategory.Id AND RentalUnit.IsDeleted=0 AND RentalUnit.IsAvailableOnline=1) = 0

                    FOR JSON PATH";
            return this.Find(commandText);
        }
    }
}
