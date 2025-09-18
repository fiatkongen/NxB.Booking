using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NxB.Domain.Common.Enums;

namespace NxB.Dto.AllocationApi
{
    public class CreateTimeSpanDto
    {
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public TimeSpanType OpenClosed { get; set; }
        public bool IsOnlineOnly { get; set; }
        public string ParameterString { get; set; }
        public int? ParameterNumber { get; set; }
        public int? GroupNo { get; set; }
    }

    public class TimeSpanDto : CreateTimeSpanDto
    {
        public Guid Id { get; set; }
        public Guid ResourceId { get; set; }
    }

    public class CreateRentalCategoryTimeSpanDto : CreateTimeSpanDto
    {
        public Guid RentalCategoryId { get; set; }
    }

    public class RentalCategoryTimeSpanDto : TimeSpanDto
    {
        public Guid RentalCategoryId { get; set; }
    }

    public class CreateOrModifyTimeSpanDto : CreateTimeSpanDto
    {
        public Guid? Id { get; set; }
        public Guid ResourceId { get; set; }
        public string Action { get; set; }
    }
}
