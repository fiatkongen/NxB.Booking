using System;
using System.Collections.Generic;
using System.Text;
using NxB.Domain.Common.Enums;

namespace NxB.Dto.TallyWebIntegrationApi
{
    public class SocketSwitchControllerDto
    {
        public int RadioAddress { get; set; }
        public bool IsOnLine { get; set; }
        public TWIModus Modus { get; set; }
        public bool IsKeyPad2Enabled { get; set; }
        public bool HasErrorKeyPad1 { get; set; }
        public bool HasErrorKeyPad2 { get; set; }
        public DateTime LastUpdate { get; set; }
    }
}
