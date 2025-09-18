using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Munk.AspNetCore;
using Munk.AspNetCore.Sql;
using NxB.Domain.Common.Interfaces;
using NxB.BookingApi.Models;

namespace NxB.BookingApi.Infrastructure
{
    public class CostIntervalRepository : TenantFilteredRepository<CostInterval, AppDbContext>, ICostIntervalRepository
    {
        private readonly IDbConnectionFactory _connectionFactory;

        public CostIntervalRepository(IClaimsProvider claimsProvider, AppDbContext appDbContext, IDbConnectionFactory connectionFactory) : base(claimsProvider, appDbContext)
        {
            _connectionFactory = connectionFactory;
        }

        public void Update(CostInterval costInterval)
        {
            AppDbContext.Update(costInterval);
        }

        public void UpdateType(Guid id, string type)
        {
            using var sqlConnection = _connectionFactory.Create();
            var cmd = new SqlCommand
            {
                CommandText = $"update costinterval set CostType = '{type}' where id = '{id}'",
                CommandType = CommandType.Text,
                Connection = sqlConnection
            };
            sqlConnection.Open();
            cmd.ExecuteScalar();
        }

        public void Add(CostInterval costInterval)
        {
            AppDbContext.Add(costInterval);
        }

        public void Add(IEnumerable<CostInterval> costIntervals)
        {
            costIntervals.ToList().ForEach(Add);
        }

        public async Task<CostInterval> FindSingle(Guid id)
        {
            var costInterval = await this.TenantFilteredEntitiesQuery.SingleAsync(x => x.Id == id);
            return costInterval;
        }

        public async Task<CostInterval> FindSingleOrDefault(Guid id)
        {
            var costInterval = await this.TenantFilteredEntitiesQuery.SingleOrDefaultAsync(x => x.Id == id);
            return costInterval;
        }

        public async Task<List<CostInterval>> FindAll()
        {
            var costIntervals = await this.TenantFilteredEntitiesQuery.Where(x => !x.IsDeleted).OrderBy(x => x.StartDate).ThenBy(x => x.EndDate).ThenBy(x => x.Cost).ToListAsync();
            return costIntervals;
        }

        public Task<List<CostInterval>> FindAllFromTenantId(Guid tenantId)
        {
            return AppDbContext.CostIntervals.Where(x => !x.IsDeleted && x.TenantId == tenantId).OrderBy(x => x.StartDate).ThenBy(x => x.EndDate).ThenBy(x => x.Cost).ToListAsync();
        }

        public Task<List<CostInterval>> FindAllPriceFromPriceProfileId(Guid priceProfileId)
        {
            return this.TenantFilteredEntitiesQuery.Where(x => !x.IsDeleted && x.PriceProfileId == priceProfileId).ToListAsync();
        }

        public void DeletePermanently(CostInterval costInterval)
        {
            this.AppDbContext.Remove(costInterval);
        }

        public void MarkAsDeleted(Guid id)
        {
            throw new NotImplementedException();
        }

        public int DeleteAll(Guid tenantId)
        {
            throw new NotImplementedException();
        }
    }
}