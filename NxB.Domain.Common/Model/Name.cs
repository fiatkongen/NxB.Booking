using System;
using Munk.Utils.Object;

namespace NxB.Domain.Common.Model
{
    [Serializable]
    public class Name : ValueObject<Name>
    {
        public string Firstname { get; set; }
        public string Lastname { get; set; }

        public Name(string firstname, string lastname)
        {
            Firstname = firstname;
            Lastname = lastname;
        }

        public new string ToString()
        {
            var fullname = (Firstname ?? "") + " " + (Lastname ?? "");
            return string.IsNullOrWhiteSpace(fullname) ? "" : fullname;
        }
    }
}