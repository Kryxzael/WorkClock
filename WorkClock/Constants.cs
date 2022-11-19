using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorkClock
{
    /// <summary>
    /// Contains common constants used throughout the program
    /// </summary>
    public static class Constants
    {
        public const int WeekLength = 5;

        public static readonly TimeSpan DayStart = new(08, 0, 0);
        public static readonly TimeSpan DayEnd   = new(16, 0, 0);

        public static DateTime Now
        {
            get
            {
                return DateTime.Now;

                DayOfWeek day = DayOfWeek.Wednesday;
                TimeSpan time = new TimeSpan(11, 20, 00);

                DateTime date = DateTime.Today;

                while (date.DayOfWeek != DayOfWeek.Monday)
                    date = date.AddDays(-1);

                while (date.DayOfWeek != day)
                    date = date.AddDays(1);

                return date + time;
            }
        }

        public static DurationProgressInfo ThisWeek
        {
            get
            {
                DateTime startDate = Now.Date;

                while (startDate.DayOfWeek != DayOfWeek.Monday)
                    startDate = startDate.AddDays(-1);

                return new DurationProgressInfo(startDate + DayStart, startDate.AddDays(WeekLength - 1) + DayEnd);
            }
        }

        public static DurationProgressInfo GetDaySpan(DateTime date)
        {
            return new DurationProgressInfo(date.Date + DayStart, date.Date + DayEnd);
        }

        public static TimeSpan GetDurationCompletedThisWeek()
        {
            TimeSpan time = default;

            //DayOfWeek starts with Sunday at 0, so we need to make an exception for it in this case
            if (Now.DayOfWeek == DayOfWeek.Sunday && System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat.FirstDayOfWeek == DayOfWeek.Monday)
                return GetTotalDurationThisWeek();

            for (DayOfWeek day = DayOfWeek.Monday; day <= DayOfWeek.Friday; day++)
            {
                if (Now.DayOfWeek != day)
                {
                    time += DayEnd - DayStart;
                }
                else
                {
                    time += GetDaySpan(Now).GetTimeSinceStart();
                    break;
                }
            }

            return time;
        }

        public static TimeSpan GetTotalDurationThisWeek()
        {
            return (DayEnd - DayStart) * WeekLength;
        }
    }
}
