using System;
using System.Collections.Generic;
using System.Text;
using NxB.Domain.Common.Enums;

namespace NxB.Domain.Common.Model
{
    public class TallyRadiosFilter
    {
        public RadioUnitsFilter SwitchesFilter { get; set; } = RadioUnitsFilter.All;
        public List<RadioAccessUnit> SwitchRadios { get; set; } = new List<RadioAccessUnit>();

        public RadioUnitsFilter SocketsFilter { get; set; } = RadioUnitsFilter.All;
        public List<RadioAccessUnit> SocketRadios { get; set; } = new List<RadioAccessUnit>();

        public void AddSwitchRadios(List<SwitchItem> switchItems)
        {
            switchItems.ForEach(x => SwitchRadios.Add(new RadioAccessUnit{ RadioAddress = x.RadioAddress, Option = x.Option }));
        }
    }
}
