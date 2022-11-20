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

        /// <summary>
        /// The time-of-day a normal workday will start
        /// </summary>
        public static readonly TimeSpan DayStart = new(8, 0, 0);

        /// <summary>
        /// The time-of-day a normal workday will end
        /// </summary>
        public static readonly TimeSpan DayEnd = new(16, 0, 0);

        /// <summary>
        /// The time-of-day the core work-hours start
        /// </summary>
        public static readonly TimeSpan CoreWorkHoursStart = new(9, 0, 0);

        /// <summary>
        /// The time-of-day the core work-hours end
        /// </summary>
        public static readonly TimeSpan CoreWorkHoursEnd = new(14, 0, 0);

        /// <summary>
        /// The time-of-day lunch break starts
        /// </summary>
        public static readonly TimeSpan LunchStart = new(11, 30, 0);

        /// <summary>
        /// The time-of-day lunch break ends
        /// </summary>
        public static readonly TimeSpan LunchEnd = new(12, 0, 0);

        /// <summary>
        /// The amount of time before lunch starts where lunch is considered soon
        /// </summary>
        public static readonly TimeSpan LunchSoonSpan = new(0, 10, 0);

        /// <summary>
        /// The amount of time before the day ends where the day ending is considered soon and the color yellow is used
        /// </summary>
        public static readonly TimeSpan DayEndSoonYellow = new(0, 1, 0);

        /// <summary>
        /// The amount of time before the day ends where the day ending is considered soon and the color red is used
        /// </summary>
        public static readonly TimeSpan DayEndSoonRed = new(0, 15, 0);

        /// <summary>
        /// The amount of time before the day ends where the day ending is considered soon and the color red and blinking is used
        /// </summary>
        public static readonly TimeSpan DayEndSoonBlink = new(0, 5, 0);

        /// <summary>
        /// Gets the current time
        /// </summary>
        public static DateTime Now
        {
            get
            {
                return DateTime.Now;

                DayOfWeek day = DayOfWeek.Saturday;
                TimeSpan time = new TimeSpan(23, 59, 59);

                DateTime date = DateTime.Today;

                while (date.DayOfWeek != DayOfWeek.Monday)
                    date = date.AddDays(-1);

                while (date.DayOfWeek != day)
                    date = date.AddDays(1);

                return date + time;
            }
        }

        /// <summary>
        /// Gets a duration progress info about the current week
        /// </summary>
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

        /// <summary>
        /// Gets a duration progress info object about the provided work-day
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        public static DurationProgressInfo GetDaySpan(DateTime date)
        {
            return new DurationProgressInfo(date.Date + DayStart, date.Date + DayEnd);
        }

        /// <summary>
        /// Gets how much time has been spent at work this week
        /// </summary>
        /// <returns></returns>
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

        /// <summary>
        /// Gets the total amount of time in a given work-week
        /// </summary>
        /// <returns></returns>
        public static TimeSpan GetTotalDurationThisWeek()
        {
            return (DayEnd - DayStart) * WeekLength;
        }
    }
}
