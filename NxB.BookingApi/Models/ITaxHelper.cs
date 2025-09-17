using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NxB.Domain.Common.Interfaces;

namespace NxB.BookingApi.Models;

public interface ITaxHelper
{
    Task AddTaxToOrder(List<ITaxableItem> taxableItems, DbContext dbContext);
    Task UpdateTaxForOrder(List<ITaxableItem> taxableItems, DbContext dbContext);
}
