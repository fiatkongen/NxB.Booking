using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NxB.Dto.ApplicationLogApi
{
    public enum FindSmsStatusEnum
    {
        None,
        Delivered,
        DeliveryUnderway,
        DeliveryFailure,
        Error
    }
}
