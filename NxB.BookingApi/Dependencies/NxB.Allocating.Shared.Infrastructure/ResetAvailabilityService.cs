using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using Microsoft.EntityFrameworkCore;
using NxB.BookingApi.Models;
using NxB.Domain.Common.Interfaces;

namespace NxB.Allocating.Shared.Infrastructure
{
    public class ResetAvailabilityService<TAppDbContext> : IResetAvailabilityService where TAppDbContext : DbContext
    {
        private readonly IClaimsProvider _claimsProvider;
        private readonly TAppDbContext _appDbContext;
        private readonly IRentalUnitRepository _rentalUnitRepository;
        private readonly IAllocationRepository _allocationRepository;
        private readonly IAvailabilityMatrixRepository _availabilityMatrixRepository;

        public ResetAvailabilityService(IClaimsProvider claimsProvider, TAppDbContext appDbContext, IRentalUnitRepository rentalUnitRepository, IAllocationRepository allocationRepository, IAvailabilityMatrixRepository availabilityMatrixRepository)
        {
            _claimsProvider = claimsProvider;
            _appDbContext = appDbContext;
            _rentalUnitRepository = rentalUnitRepository;
            _allocationRepository = allocationRepository;
            _availabilityMatrixRepository = availabilityMatrixRepository;
        }

        public async Task ResetAvailability(bool skipResetAvailabilityMatrix = false)
        {
            if (!skipResetAvailabilityMatrix) await ResetAvailabilityMatrix();

            var validRentalUnits = await _rentalUnitRepository.FindAll();
            var validRentalUnitIds = validRentalUnits.Select(x => x.Id).ToList();

            var allocationRentalUnitIds = await _allocationRepository.FindAllRentalUnitIds();
            var missingRentalUnits = validRentalUnits.Where(x => !allocationRentalUnitIds.Contains(x.Id)).ToList();

            CreateMissingBaseAllocations(missingRentalUnits);

            var excessRentalUnitIds = allocationRentalUnitIds.Where(x => !validRentalUnitIds.Contains(x)).ToList();
            var excessRentalUnits = await _rentalUnitRepository.FindAllFromAllocationIds(excessRentalUnitIds);
            await RemoveExcessBaseAllocations(excessRentalUnits);
        }

        private async Task ResetAvailabilityMatrix(Guid? tenantId = null)
        {
            await this._appDbContext.Database.ExecuteSqlRawAsync(
                $"DELETE FROM AvailabilityMatrix WHERE TenantId = '{tenantId ?? _claimsProvider.GetTenantId()}'");
        }

        private void CreateMissingBaseAllocations(List<RentalUnit> rentalUnits)
        {
            foreach (var rentalUnit in rentalUnits)
            {
                var tenantId = _claimsProvider.GetTenantId();
                var baseAllocation = new Allocation(Guid.NewGuid(), tenantId, rentalUnit.Id, rentalUnit.Name,
                    Allocation.BaseAllocationDateInterval, 1);

                _allocationRepository.Add(baseAllocation);
            }
        }

        private async Task RemoveExcessBaseAllocations(List<RentalUnit> rentalUnits)
        {
            var rentalUnitIds = rentalUnits.Select(x => x.Id).ToList();
            await _allocationRepository.DeleteAllocationsForRentalUnitIds(rentalUnitIds);
        }
    }
}
