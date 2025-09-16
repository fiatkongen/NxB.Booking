using System;
using System.Collections.Generic;

namespace NxB.Dto.AccountingApi
{

    public class CreateAccountChartItemDto
    {
        public int Number{ get; set; }
        public string Name { get; set; }
        public AccountChartType AccountChartType { get; set; }
        public AccountChartItemType AccountChartItemType { get; set; }
    }

    public class ModifyAccountChartItemDto
    {
        public Guid Id { get; set; }
        public int Number { get; set; }
        public string Name { get; set; }
    }

    public class AccountChartItemDto : CreateAccountChartItemDto
    {
        public Guid Id { get; set; }
        public List<Guid> AccountChartItemPriceProfiles { get; set; }
    }
}
