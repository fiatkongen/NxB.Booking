using System;
using System.Collections.Generic;
using System.Text;

namespace NxB.Dto.AllocationApi
{
    public class AllocationDto
    {
        public Guid Id { get; set; }
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public decimal Number { get; set; }
        public int Duration { get; set; }
        public Guid RentalUnitId { get; set; }
        public string RentalUnitName { get; set; }
    }
}
