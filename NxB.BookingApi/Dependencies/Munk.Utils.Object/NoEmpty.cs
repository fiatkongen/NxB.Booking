using System;
using System.ComponentModel.DataAnnotations;

namespace Munk.Utils.Object
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = false)]
    public class NoEmpty : ValidationAttribute
    {
        public override bool IsValid(System.Object value)
        {
            if (value == null)
                return false;

            if (Guid.TryParse(value.ToString(), out var parsedGuid))
            {
              return parsedGuid != Guid.Empty;
            }

            return !string.IsNullOrEmpty(value.ToString());
        }
    }
}