using System;
using System.Collections.Generic;
using System.Text;

namespace NxB.Settings.Shared.Infrastructure
{
    public class SettingsException : Exception
    {
        public SettingsException()
        {

        }

        public SettingsException(string message)
            : base(message)
        {
        }

        public SettingsException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}