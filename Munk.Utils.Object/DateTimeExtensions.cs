using System;
using System.Net.NetworkInformation;
using System.Text.RegularExpressions;
using Itenso.TimePeriod;
using Newtonsoft.Json;

namespace System
{
    public static class DateTimeExtensions
    {
        public static TimeZoneInfo euTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Central European Standard Time");

        public static string ToSql(this DateTime date)
        {
            return date.ToString("MM/dd/yyyy HH:mm:ss").Replace(".", ":");
        }

        public static string ToJsonDateString(this DateTime date)
        {
            return date.ToString("yyyy-MM-ddT00:00:00").Replace(".", ":");
        }

        public static string ToJsonDateTimeString2(this DateTime date)
        {
            string formattedDateTime = JsonConvert.SerializeObject(date, new JsonSerializerSettings
            {
                DateFormatHandling = DateFormatHandling.IsoDateFormat,
                DateTimeZoneHandling = DateTimeZoneHandling.Local
            });
            // Use a regular expression to match the pattern between '.' and '+'
            string result = Regex.Replace(formattedDateTime, @"\..+?\+", "+").Replace("\"", "");
            return result;
        }

        public static string ToDanishDate(this DateTime date)
        {
            return date.ToString("dd.MM.yyyy");
        }

        public static string ToDanishDateTime(this DateTime date)
        {
            return date.ToString("dd.MM.yyyy H:mm");
        }

        public static string ToDanishDateTime2(this DateTime date)
        {
            return date.ToString("d.M.yyyy H:mm");
        }

        public static string ToDanishDateTimeShort(this DateTime date)
        {
            return date.ToString("dd.MM.yy HH:mm");
        }

        public static string ToUtcString(this DateTime date)
        {
            return date.ToUniversalTime().ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fff'Z'");
        }

        public static int GetMonthDiff(this DateTime startDate, DateTime endDate)
        {
            int monthsApart = 12 * (startDate.Year - endDate.Year) + startDate.Month - endDate.Month;
            return Math.Abs(monthsApart);
        }

        public static int GetDaysDiff(this DateTime startDate, DateTime endDate)
        {
            return endDate.Subtract(startDate).Days;
        }

        public static int GetDaysDiffAbs(this DateTime startDate, DateTime endDate)
        {
            return Math.Abs(GetDaysDiff(startDate, endDate));
        }

        public static DateTime ToEuTimeZone(this DateTime date)
        {
            DateTime euTime = TimeZoneInfo.ConvertTime(date, TimeZoneInfo.Local, euTimeZone);
            return euTime;
        }

        public static DateTime ToEuTimeZoneFromUtc(this DateTime date)
        {
            DateTime euTime = TimeZoneInfo.ConvertTime(date, TimeZoneInfo.Utc, euTimeZone);
            return euTime;
        }

        public static DateTime ToUtcFromEuTimeZone(this DateTime date)
        {
            DateTime euTime = TimeZoneInfo.ConvertTimeToUtc(date, euTimeZone);
            return euTime;
        }

        public static DateTime ToEuTimeZoneFromUnixMs(this long unixTimeMs)
        {
            DateTimeOffset dateTimeOffset = DateTimeOffset.FromUnixTimeMilliseconds(unixTimeMs);
            DateTime dateTime = dateTimeOffset.UtcDateTime.ToEuTimeZoneFromUtc();
            return dateTime;

            //DateTimeOffset dateTimeOffset = DateTimeOffset.FromUnixTimeMilliseconds(unixTimeMs);
            //DateTime localDateTime = dateTimeOffset.ToLocalTime().DateTime;
            //return localDateTime;
        }

        public static int GetQuarter(this DateTime dateTime)
        {
            if (dateTime.Month <= 3)
                return 1;

            if (dateTime.Month <= 6)
                return 2;

            if (dateTime.Month <= 9)
                return 3;

            return 4;
        }

        public static DateTime GetEndDayForQuarter(this DateTime dateTime)
        {
            var result =
                dateTime.Date
                    .AddDays(1 - dateTime.Day)
                    .AddMonths(3 - (dateTime.Month - 1) % 3)
                    .AddDays(-1);
            return result;
        }

        public static DateTime GetStartDayForQuarter(this DateTime dateTime)
        {
            if (dateTime.Month <= 3)
                return new DateTime(dateTime.Year, 1, 1);

            if (dateTime.Month <= 6)
                return new DateTime(dateTime.Year, 4, 1);

            if (dateTime.Month <= 9)
                return new DateTime(dateTime.Year, 7, 1);

            return new DateTime(dateTime.Year, 10, 1);
        }
    }
}
