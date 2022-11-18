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

        public static string CreateBar(int inclusiveWidth, float fillPercentage, char start = '[', char fill = '#', char empty = ' ', char end = ']')
        {
            if (inclusiveWidth < 2)
                throw new ArgumentOutOfRangeException(nameof(inclusiveWidth), "Inclusive width must be at least 2 to account for the start and end caps");

            fillPercentage = Math.Clamp(fillPercentage, 0f, 1f);

            StringBuilder str = new StringBuilder(new string(empty, inclusiveWidth));
            str[0] = start;
            str[^1] = end;

            int exclusiveWidth = inclusiveWidth - 2;

            if (exclusiveWidth > 0)
            {
                for (int i = 0; i < Math.Round(fillPercentage * exclusiveWidth); i++)
                    str[i + 1] = fill;
            }

            return str.ToString();
        }

        public static string CreateTableEntry(string text, string val)
        {
            return text.PadRight(20) + val.PadLeft(20);
        }

        public static string CreateTableEntry(string text, DateTime time)
        {
            return CreateTableEntry(text, time.ToString("HH:mm:ss"));
        }

        public static string CreateTableEntry(string text, TimeSpan span)
        {
            if (span.Ticks < 0)
                span = new TimeSpan(-span.Ticks);

            else if (span == default)
                return CreateTableEntry(text, "Zero");

            return CreateTableEntry(text, (int)span.TotalHours + span.ToString("':'mm':'ss"));
        }

        public record DurationProgressInfo(DateTime Start, DateTime End)
        {
            public DurationProgressInfo(DateTime start, TimeSpan duration) : this(start, start + duration)
            {  }

            public TimeSpan GetTimeSinceStart()
            {
                return Now - Start;
            }

            public TimeSpan GetTimeUntilEnd()
            {
                return End - Now;
            }

            public float GetCompletionPercentage()
            {
                return (Now.Ticks - Start.Ticks) / (float)(End - Start).Ticks;
            }

            public string CreateBar(int inclusiveWidth, char start = '[', char fill = '#', char empty = ' ', char end = ']')
            {
                return Constants.CreateBar(inclusiveWidth, GetCompletionPercentage(), start, fill, empty, end);
            }
        }
    }
}
