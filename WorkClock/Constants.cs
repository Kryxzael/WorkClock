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
        public static readonly TimeSpan DayEndSoonYellow = new(1, 0, 0);

        /// <summary>
        /// The amount of time before the day ends where the day ending is considered soon and the color red is used
        /// </summary>
        public static readonly TimeSpan DayEndSoonRed = new(0, 15, 0);

        /// <summary>
        /// The amount of time before the day ends where the day ending is considered soon and the color red and blinking is used
        /// </summary>
        public static readonly TimeSpan DayEndSoonBlink = new(0, 5, 0);

        /// <summary>
        /// The amount of time before the next meeting where it is considered soon and the color yellow is used
        /// </summary>
        public static readonly TimeSpan MeetingSoonYellow = new(0, 10, 0);

        /// <summary>
        /// The amount of time before the next meeting where it is considered soon and the color red is used
        /// </summary>
        public static readonly TimeSpan MeetingSoonRed = new(0, 5, 0);

        /// <summary>
        /// The amount of time before the next meeting where it is considered soon and the color red and blinking is used
        /// </summary>
        public static readonly TimeSpan MeetingSoonBlink = new(0, 1, 0);

        /// <summary>
        /// The amount of time to adjust the end-time by if the --adjust flag has been passed to the program
        /// </summary>
        public static TimeSpan AdjustEndTimeSpan { get; } = -new TimeSpan(0, 15, 0);
    }
}
