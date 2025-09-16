using System;
using System.Linq;
using System.Text;

namespace Munk.AspNetCore.Cryptography
{
	public class Modulus10
	{
		public static int CreateCheckDigit(String idWithoutCheckdigit)
		{
			// allowable characters within identifier
			String validChars = "0123456789ABCDEFGHIJKLMNOPQRSTUVYWXZ_";

			// remove leading or trailing whitespace, convert to uppercase
			idWithoutCheckdigit = idWithoutCheckdigit.Trim();

			// this will be a running total
			int sum = 0;

			// loop through digits from right to left
			for (int i = 0; i < idWithoutCheckdigit.Length; i++)
			{

				//set ch to "current" character to be processed
				char ch = idWithoutCheckdigit
						  .ElementAt(idWithoutCheckdigit.Length - i - 1);

				// our "digit" is calculated using ASCII value - 48
				int digit = (int)ch - 48;

				// weight will be the current digit's contribution to
				// the running total
				int weight;
				if (i % 2 == 0)
				{

					// for alternating digits starting with the rightmost, we
					// use our formula this is the same as multiplying x 2 and
					// adding digits together for values 0 to 9.  Using the
					// following formula allows us to gracefully calculate a
					// weight for non-numeric "digits" as well (from their
					// ASCII value - 48).
					weight = (2 * digit) - (int)(digit / 5) * 9;

				}
				else
				{

					// even-positioned digits just contribute their ascii
					// value minus 48
					weight = digit;

				}

				// keep a running total of weights
				sum += weight;

			}

			// avoid sum less than 10 (if characters below "0" allowed,
			// this could happen)
			sum = Math.Abs(sum) + 10;

			// check digit is amount needed to reach next number
			// divisible by ten
			return (10 - (sum % 10)) % 10;
		}


		//--------------------------------
		// Filter out non-digit characters
		//--------------------------------

		private static String GetDigitsOnly(String s)
		{
			StringBuilder digitsOnly = new StringBuilder();
			char c;
			for (int i = 0; i < s.Length; i++)
			{
				c = s.ElementAt(i);
				if (Char.IsDigit(c))
				{
					digitsOnly.Append(c);
				}
			}
			return digitsOnly.ToString();
		}

		//-------------------
		// Perform Luhn check
		//-------------------

		public static bool IsValid(String number)
		{
			String digitsOnly = GetDigitsOnly(number);
			int sum = 0;
			int digit = 0;
			int addend = 0;
			bool timesTwo = false;

			for (int i = digitsOnly.Length - 1; i >= 0; i--)
			{
				digit = int.Parse(digitsOnly.Substring(i, 1));
				if (timesTwo)
				{
					addend = digit * 2;
					if (addend > 9)
					{
						addend -= 9;
					}
				}
				else
				{
					addend = digit;
				}
				sum += addend;
				timesTwo = !timesTwo;
			}

			int modulus = sum % 10;
			return modulus == 0;

		}
	}
}
