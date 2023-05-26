using System;

namespace Core.Domain.Infrastructure.Services
{
    public class DateTimeService : IDateTimeService
    {
        public DateTime Now => DateTime.UtcNow.ToInstanceDate();
        public DateTime Today => Now.Date;
        public DateTime Tomorrow => Today.AddDays(1);
        public DateTime Yesterday => Today.AddDays(-1);
        public DateTime MaxYesterday => Today.AddMilliseconds(-1);
        public DateTime MaxToday => Today.AddDays(1).AddMilliseconds(-1);    
    }



    public static class DateExtensions
    {
        public static DateTime ToInstanceDate(this DateTime value)
        {
            TimeZoneInfo currentTimeZone = TimeZoneInfo.FindSystemTimeZoneById("E. Africa Standard Time");
            return TimeZoneInfo.ConvertTimeFromUtc(value, currentTimeZone);
        }
    }
}
