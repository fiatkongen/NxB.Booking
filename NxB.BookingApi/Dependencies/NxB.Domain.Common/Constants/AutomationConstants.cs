using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NxB.Domain.Common.Constants
{
    public static class AutomationConstants
    {
        public static string EVENT_TYPE_ACCESS = "access";
        public static string EVENT_NAME_GRANTED = "granted";
        public static string EVENT_NAME_DENIED = "denied";
        public static string SENDER_CAM_IN = "anpr-enter";
        public static string SENDER_CAM_OUT = "anpr-exit";
    }
}
