using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace NxB.Dto.AutomationApi
{
    public class HomeSeerDevices
    {
        public string Name { get; set; }
        public List<HomeSeerZWaveDevice> Devices { get; set; }
    }

    public class HomeSeerZWaveDevice
    {
        public bool IsController() => this.Device_type_string == "Z-Wave Switch Root Device";
        public bool IsMeter => this.Device_type_string == "Z-Wave Electric Meter";
        public bool IsSwitch => this.Device_type_string == "Z-Wave Switch Binary";

        public DateTime LastChange
        {
            get
            {
                var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                const string pattern = @"/Date\((-?\d+)([+-]\d{4})?\)/";
                var match = Regex.Match(Last_change, pattern);

                var milliseconds = long.Parse(match.Groups[1].Value);
                var dateTimeOffset = new DateTimeOffset(epoch.AddMilliseconds(milliseconds));
                return dateTimeOffset.LocalDateTime;
            }
        }

        public bool IsOn
        {
            get
            {
                if (!IsSwitch)
                {
                    return false;
                }
                return (int)Value == 255;
            }
        }

        public int GetChildIndex(HomeSeerZWaveDevice child)
        {
            if (!IsController())
            {
                throw new Exception("Cannot access ChildIndex for " + Device_type_string);
            }

            if (child.IsSwitch)
            {
                return this.GetChildSwitches().IndexOf(child);
            }

            if (child.IsMeter)
            {
                return this.GetChildMeters().IndexOf(child);
            }

            throw new Exception("Cannot access ChildIndex for " + Device_type_string);
        }

        public int Ref { get; set; }
        public string Name { get; set; }
        public string Location { get; set; }
        public string Location2 { get; set; }
        public double Value { get; set; }
        public string Status { get; set; }
        public string Device_type_string { get; set; }
        public string Last_change { get; set; }
        public int Relationship { get; set; }
        public bool Hide_from_view { get; set; }
        public int[] Associated_devices { get; set; }
        public HomeSeerZWaveDeviceType HomeSeerZWaveDeviceType { get; set; }
        public object Device_type_values { get; set; }
        public string UserNote { get; set; }
        public string UserAccess { get; set; }
        public string Status_image { get; set; }
        public string Voice_command { get; set; }
        public int Misc { get; set; }
        public string Interface_name { get; set; }
        public List<HomeSeerZWaveDevice> Children { get; set; }

        public List<HomeSeerZWaveDevice> GetChildSwitches()
        {
            return Children.Where(x => x.IsSwitch).ToList();
        }

        public List<HomeSeerZWaveDevice> GetChildMeters()
        {
            return Children.Where(x => x.IsMeter).ToList();
        }
    }
}
