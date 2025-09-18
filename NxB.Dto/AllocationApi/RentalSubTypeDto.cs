using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NxB.Dto.AllocationApi
{
    public class CreateRentalSubTypeDto
    {
        public decimal? Size { get; set; }
        public RentalSubTypeEnum Type { get; set; }
        public string IconName { get; set; }
        public Dictionary<string, string> NameTranslations { get; set; }
        public Dictionary<string, string> DescriptionTranslations { get; set; }
    }

    public class ModifyRentalSubTypeDto : CreateRentalSubTypeDto
    {
        public Guid Id { get; set; }
    }

    public class RentalSubTypeDto : CreateRentalSubTypeDto
    {
        public Guid Id { get; set; }
        public bool IsDeleted { get; set; }
        public List<RentalSubTypeLinkDto> RentalSubTypeLinks { get; set; } = new();
    }

    public class RentalSubTypeLinkDto
    {
        public Guid RentalCategoryId { get; set; }
        public Guid RentalSubTypeId { get; set; }
    }

    public enum RentalSubTypeEnum
    {
        None,
        EstateType,
        Badge
    }
}
