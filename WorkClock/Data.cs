using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorkClock
{
    /// <summary>
    /// Stores dynamic/changing data used by the rest of the program
    /// </summary>
    public static class Data
    {
        public static TimeSpan TodayOffset { get; set; } = default;

        public static bool AdjustEndTime { get; set; }

        public static TimeSpan TodayStart
        {
            get
            {
                return Constants.DayStart + TodayOffset;
            }
        }

        public static TimeSpan TodayEnd
        {
            get
            {
                return TodayEndUnadjusted + (AdjustEndTime ? Constants.AdjustEndTimeSpan : default);
            }
        }

        public static TimeSpan TodayEndUnadjusted
        {
            get
            {
                return Constants.DayEnd + TodayOffset;
            }
        }

        public static DurationProgressInfo TodaySpan
        {
            get
            {
                return new DurationProgressInfo(Now.Date + TodayStart, Now.Date + TodayEnd);
            }
        }


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

                return GetDateAtWeekDay(day) + time;
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

                return new DurationProgressInfo(startDate + Constants.DayStart, startDate.AddDays(Constants.WeekLength - 1) + Constants.DayEnd);
            }
        }

        /// <summary>
        /// Gets the meetings passed as arguments to the program
        /// </summary>
        public static List<Meeting> Meetings { get; set; } = new();

        /// <summary>
        /// Gets whether there is a meeting between the provided points in time
        /// </summary>
        /// <returns></returns>
        public static bool InMeeting(DateTime start, DateTime end) 
        {
            foreach (Meeting i in Meetings)
            {
                if (start < i.EndTime && end > i.StartTime)
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Gets the date-time instance of a given day-of-the-week this week
        /// </summary>
        /// <param name="dow"></param>
        /// <returns></returns>
        public static DateTime GetDateAtWeekDay(DayOfWeek dow)
        {
            DateTime date = Now.Date;

            while (date.DayOfWeek != DayOfWeek.Monday)
                date = date.AddDays(-1);

            while (date.DayOfWeek != dow)
                date = date.AddDays(1);

            return date;
        }

        /// <summary>
        /// Gets whether time values should be rendered in a human natural way instead of using time-stamp notation.
        /// </summary>
        public static bool HumanReadableTimes { get; set; } = true;

        /// <summary>
        /// Gets a duration progress info object about the provided work-day
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        public static DurationProgressInfo GetDaySpan(DateTime date)
        {
            return new DurationProgressInfo(date.Date + Constants.DayStart, date.Date + Constants.DayEnd);
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
                    time += Constants.DayEnd - Constants.DayStart;
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
            return (Constants.DayEnd - Constants.DayStart) * Constants.WeekLength;
        }
    }
}
