using System;
using System.Collections.Generic;
using System.Text;
using NxB.Domain.Common.Enums;

namespace NxB.BookingApi.Models
{
    [Serializable]
    public class TConRadioAccessCode
    {
        public int Idx { get; private set; }
        public byte _New { get; set; }
        public int _RadioAddr { get; set; }
        public int _Code { get; set; }
        public bool _KeyCode { get; set; }
        public bool _Active { get; set; }
        public byte _Option { get; set; }

        public void Remove()
        {
            _New = 1;
            _Active = false;
        }
    }
}
