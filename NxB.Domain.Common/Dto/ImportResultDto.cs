using System;
using System.Collections.Generic;
using System.Text;

namespace NxB.Domain.Common.Dto
{
    public class ImportResultDto
    {
        public int CreatedCount { get; set; }
        public int ModifiedCount { get; set; }
        public int DeletedCount { get; set; }
    }
}
