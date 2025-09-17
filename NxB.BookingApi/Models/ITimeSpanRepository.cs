using NxB.Domain.Common.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NxB.BookingApi.Models
{
    public interface ITimeSpanRepository : ICloneWithCustomClaimsProvider<ITimeSpanRepository>
    {
        void Add(TimeSpanBase timeSpan);
        void Update(TimeSpanBase timeSpan);
        void DeletePermanently(TimeSpanBase timeSpan);
        Task<T> FindSingleOrDefault<T>(Guid id) where T : TimeSpanBase;
        Task<List<T>> FindAll<T>() where T : TimeSpanBase;
        Task<List<T>> FindAllWithin<T>(DateInterval dateInterval) where T : TimeSpanBase;
        Task<List<T>> FindAllWithinForTenant<T>(Guid tenantId, DateInterval dateInterval) where T : TimeSpanBase;

    }
}
