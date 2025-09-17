using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using NxB.BookingApi.Models;
using NxB.Domain.Common.Interfaces;

namespace NxB.BookingApi.Infrastructure
{
    public class RentalAvailabilityCacheFactory : IRentalCacheProvider
    {
        private readonly IAvailabilityMatrixRepository _availabilityMatrixRepository;
        private readonly AvailabilityMatrixFactory _availabilityMatrixFactory;
        private readonly IClaimsProvider _claimsProvider;
        private readonly IStartDateTimeChunkDivider _startDateTimeChunkDivider;
        private readonly IRentalUnitRepository _rentalUnitRepository;
        private IAvailabilityCache _rentalUnitsCache;
        private IAvailabilityCache _rentalCategoriesCache;
        private readonly IAllocationRepositoryCached _allocationRepositoryCached;

        public RentalAvailabilityCacheFactory(
            IAvailabilityMatrixRepository availabilityMatrixRepository,
            AvailabilityMatrixFactory availabilityMatrixFactory,
            IClaimsProvider claimsProvider,
            IStartDateTimeChunkDivider startDateTimeChunkDivider,
            IAllocationRepositoryCached allocationRepositoryCached,
            IRentalUnitRepository rentalUnitRepository
        )
        {
            _availabilityMatrixRepository = availabilityMatrixRepository;
            _availabilityMatrixFactory = availabilityMatrixFactory;
            _claimsProvider = claimsProvider;
            _startDateTimeChunkDivider = startDateTimeChunkDivider;
            _allocationRepositoryCached = allocationRepositoryCached;
            _rentalUnitRepository = rentalUnitRepository;
        }

        public IAvailabilityCache RentalUnitsCache => _rentalUnitsCache ?? (_rentalUnitsCache =
                                                          InstantiateAvailabilityCache("ru",
                                                              new AvailabilitySeeder(_allocationRepositoryCached,
                                                                  new RentalUnitsToCachedAllocationsConverter())));

        public IAvailabilityCache RentalCategoriesCache => _rentalCategoriesCache ?? (_rentalCategoriesCache =
                                                               InstantiateAvailabilityCache("rc",
                                                                   new AvailabilitySeeder(_allocationRepositoryCached,
                                                                       new RentalCategoriesToCachedAllocationsConverter(
                                                                           _rentalUnitRepository))));

        private AvailabilityCache InstantiateAvailabilityCache(string uniquedCacheId, IAvailabilitySeeder availabilitySeeder)
        {
            return new AvailabilityCache(new TimeChunkMonthDivider(_claimsProvider, _startDateTimeChunkDivider, uniquedCacheId), availabilitySeeder, _availabilityMatrixRepository, _availabilityMatrixFactory);
        }
    }
}