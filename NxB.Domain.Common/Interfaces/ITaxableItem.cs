using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NxB.Domain.Common.Interfaces
{
    public interface ITaxableItem
    {
        decimal Tax { get; set; }
        decimal TaxPercent { get; set; }
        Guid ResourceId { get; set; }
        decimal Total { get; }
    }
}
