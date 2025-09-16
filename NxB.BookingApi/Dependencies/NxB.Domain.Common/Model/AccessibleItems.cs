using System;
using System.Collections.Generic;
using System.Linq;

namespace NxB.Domain.Common.Model
{
    public class AccessItem
    {
        public Guid AccessGroupId { get; set; }
        public string Name { get; set; }
    }

    public class SwitchItem
    {
        public int RadioAddress { get; set; }
        public string Name { get; set; }
        public int Option { get; set; } = 0;
    }

    public class AccessibleItems
    {
        public List<AccessItem> AccessItems { get; set; } = new List<AccessItem>();
        public List<SwitchItem> SwitchItems { get; set; } = new List<SwitchItem>();

        public bool IsEmpty => AccessItems.Count == 0 && SwitchItems.Count == 0;

        public string GetAccessNames()
        {
            var accessItemNames = AccessItems.Select(x => x.Name);
            var switchItemNames = SwitchItems.Select(x => "[" + x.RadioAddress + "] " + x.Name + "(" + x.Option + ")");
            var names = string.Join(", ", accessItemNames.Concat(switchItemNames));
            return names;
        }
    }
}
