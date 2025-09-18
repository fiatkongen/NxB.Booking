using System;
using NxB.Domain.Common.Enums;

namespace NxB.BookingApi.Models
{
    // TODO: Implement SocketSwitchController domain model - placeholder to fix compilation errors
    // This class should represent socket switch controller functionality
    public class SocketSwitchController
    {
        public int RadioAddress { get; set; }
        public TWIModus Modus { get; set; }
        public bool Keypad2Enabled { get; set; }
        public string Name { get; set; }
        public bool IsOnline { get; set; }

        // TODO: Implement domain logic methods
        public SocketSwitchController()
        {
        }

        public SocketSwitchController(int radioAddress, TWIModus modus, bool keypad2Enabled, string name, bool isOnline)
        {
            RadioAddress = radioAddress;
            Modus = modus;
            Keypad2Enabled = keypad2Enabled;
            Name = name;
            IsOnline = isOnline;
        }
    }
}