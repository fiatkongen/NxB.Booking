using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Munk.AspNetCore.Validators
{
    public static class EmailValidator
    {
        /// <summary>
        ///  ref.: https://html.spec.whatwg.org/multipage/forms.html#valid-e-mail-address (HTML5 living standard, willful violation of RFC 3522)
        /// </summary>
        public static readonly string EmailValidation_Regex = @"^[a-zA-Z0-9.!#$%&'*+\/=?^_`{|}~-]+@[a-zA-Z0-9](?:[a-zA-Z0-9-]{0,61}[a-zA-Z0-9])?(?:\.[a-zA-Z0-9](?:[a-zA-Z0-9-]{0,61}[a-zA-Z0-9])?)*$";

        public static readonly Regex EmailValidation_Regex_Compiled = new Regex(EmailValidation_Regex, RegexOptions.IgnoreCase);

        public static readonly string EmailValidation_Regex_JS = $"/{EmailValidation_Regex}/";

        /// <summary>
        /// Checks if the given e-mail is valid using various techniques
        /// </summary>
        /// <param name="email">The e-mail address to check / validate</param>
        /// <param name="useRegEx">TRUE to use the HTML5 living standard e-mail validation RegEx, FALSE to use the built-in validator provided by .NET (default: FALSE)</param>
        /// <param name="requireDotInDomainName">TRUE to only validate e-mail addresses containing a dot in the domain name segment, FALSE to allow "dot-less" domains (default: FALSE)</param>
        /// <returns>TRUE if the e-mail address is valid, FALSE otherwise.</returns>
        public static bool IsValidEmailAddress(string email, bool useRegEx = false, bool requireDotInDomainName = true)
        {
            var isValid = useRegEx
                // see RegEx comments
                ? email is not null && EmailValidation_Regex_Compiled.IsMatch(email)

                // ref.: https://stackoverflow.com/a/33931538/1233379
                : new EmailAddressAttribute().IsValid(email);

            if (isValid && requireDotInDomainName)
            {
                var arr = email.Split('@', StringSplitOptions.RemoveEmptyEntries);
                isValid = arr.Length == 2 && arr[1].Contains(".");

                if (isValid)
                {
                    var arr2 = arr[1].Split('.');
                    isValid = arr2[0].Length > 0 && arr2[1].Length > 0;
                }
            }
            return isValid;
        }
    }
}
