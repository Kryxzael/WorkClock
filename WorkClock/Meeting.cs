using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Schema;

namespace WorkClock
{
    /// <summary>
    /// Represents a meeting that can show up in the calendar view
    /// </summary>
    public class Meeting
    {
        /// <summary>
        /// Gets the starting time of the meeting
        /// </summary>
        public DateTime StartTime { get; set; }

        /// <summary>
        /// Gets the end time of the meeting
        /// </summary>
        public DateTime EndTime { get; set; }

        /// <summary>
        /// Gets how long the meeting is
        /// </summary>
        public TimeSpan Duration 
        { 
            get
            {
                return EndTime - StartTime;
            } 
        }

        /// <summary>
        /// Creates a new meeting describing the provided time-span
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        public Meeting(DateTime start, DateTime end)
        {
            StartTime = start;
            EndTime = end;
        }

        /// <summary>
        /// Creates a new, blank meeting ready for initialization by other means
        /// </summary>
        private Meeting()
        {

        }

        /// <summary>
        /// Gets a duration progress info object describing the meeting
        /// </summary>
        /// <returns></returns>
        public DurationProgressInfo GetProgress()
        {
            return new DurationProgressInfo(StartTime, EndTime);
        }

        /// <summary>
        /// Parses the provided string (usually a command line arg) as a meeting and return it
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static Meeting Parse(string value)
        {
            value = value.Trim().ToLower();

            Meeting output = new Meeting();

            string dateValue;
            string startValue;
            string endValue;

            /*
             * Find format of meeting string
             */
            if (value.Contains('-'))
            {
                string[] split = value.Split('-');
                startValue = split[0].Trim();
                endValue = split[1].Trim();
            }
            else
            {
                startValue = value;
                endValue = "1h"; //Default to 1 hour
            }

            if (startValue.Contains(' '))
            {
                string[] split = startValue.Split(' ');
                dateValue = split[0].Trim();
                startValue = split[1].Trim();
            }
            else
            {
                dateValue = "today";
            }

            /*
             * Parse date
             */
            dateValue = dateValue.ToLower();

            switch (dateValue)
            {
                case "today":
                    output.StartTime = Data.Now.Date;
                    break;

                case "yesterday":
                    output.StartTime = Data.Now.Date.AddDays(-1);
                    break;

                case "tomorrow":
                    output.StartTime = Data.Now.Date.AddDays(1);
                    break;

                case "monday" or "mon":
                    output.StartTime = Data.GetDateAtWeekDay(DayOfWeek.Monday);
                    break;

                case "tuesday" or "tue":
                    output.StartTime = Data.GetDateAtWeekDay(DayOfWeek.Tuesday);
                    break;

                case "wednesday" or "wed":
                    output.StartTime = Data.GetDateAtWeekDay(DayOfWeek.Wednesday);
                    break;

                case "thursday" or "thu":
                    output.StartTime = Data.GetDateAtWeekDay(DayOfWeek.Thursday);
                    break;

                case "friday" or "fri":
                    output.StartTime = Data.GetDateAtWeekDay(DayOfWeek.Friday);
                    break;

                default:
                    throw new FormatException("Invalid date format");
            }

            /*
             * Parse start and end time
             */
            output.StartTime += parseTimeStamp(startValue, output.StartTime.Date == Data.Now.Date);

            try
            {
                //Could be a time-stamp
                output.EndTime = output.StartTime.Date;
                output.EndTime += parseTimeStamp(endValue, output.StartTime.Date == Data.Now.Date);
            }
            catch (FormatException)
            {
                //Could be a duration
                if (endValue.EndsWith('h'))
                {
                    endValue = endValue.TrimEnd('h');
                }

                if (double.TryParse(endValue, NumberStyles.Float, CultureInfo.InvariantCulture, out double result))
                {
                    output.EndTime = output.StartTime + TimeSpan.FromHours(result);
                }
            }

            return output;

            static TimeSpan parseTimeStamp(string timeStamp, bool useAdjustedStartEnd)
            {
                //Special text
                switch (timeStamp)
                {
                    case "noon":
                        return new TimeSpan(12, 0, 0);

                    case "midnight":
                        return new TimeSpan(0, 0, 0);

                    case "start":
                        if (useAdjustedStartEnd)
                            return Data.TodayStart;

                        return Constants.DayStart;

                    case "end":
                        if (useAdjustedStartEnd)
                            return Data.TodayEnd;

                        return Constants.DayEnd;

                    case "lunch":
                        return Constants.LunchStart;
                }

                //24h int
                if (int.TryParse(timeStamp, NumberStyles.Integer, CultureInfo.InvariantCulture, out int intResult))
                {
                    if (intResult is >= 0 and < 24)
                        return TimeSpan.FromHours(intResult);

                    else
                        throw new FormatException("Invalid time format");
                }

                //Timestamp
                else if (DateTime.TryParse(timeStamp, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime dtResult))
                {
                    if (dtResult.Date != Data.Now.Date)
                        throw new FormatException("Invalid time format");

                    return dtResult.TimeOfDay;
                }

                //AM
                else if (timeStamp.EndsWith("am") || timeStamp.EndsWith("a"))
                {
                    timeStamp = timeStamp.TrimEnd('a', 'm');

                    if (int.TryParse(timeStamp, NumberStyles.Integer, CultureInfo.InvariantCulture, out int amTime))
                    {
                        if (amTime is >= 1 and < 12)
                            return TimeSpan.FromHours(amTime);

                        else if (amTime != 12)
                            return default(TimeSpan);

                        else
                            throw new FormatException("Invalid time format");

                    }
                    else
                    {
                        throw new FormatException("Invalid time format");
                    }
                }

                //PM
                else if (timeStamp.EndsWith("pm") || timeStamp.EndsWith("p"))
                {
                    timeStamp = timeStamp.TrimEnd('p', 'm');

                    if (int.TryParse(timeStamp, NumberStyles.Integer, CultureInfo.InvariantCulture, out int pmTime))
                    {
                        if (pmTime is >= 1 and < 12)
                            return TimeSpan.FromHours(pmTime + 12);

                        else if (pmTime == 12)
                            return TimeSpan.FromHours(12);

                        else
                            throw new FormatException("Invalid time format");

                    }
                    else
                    {
                        throw new FormatException("Invalid time format");
                    }
                }
                else
                {
                    throw new FormatException("Invalid time format");
                }
            }
        }

        /// <summary>
        /// Tries to pars the provided string (usually a command line arg). Returns false if the parsing fails
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool TryParse(string value, out Meeting result)
        {
            try
            {
                result = Parse(value);
                return true;
            }
            catch (FormatException)
            {
                result = null;
                return false;
            }
        }

        public override string ToString()
        {
            return $"{StartTime}-{EndTime.ToShortTimeString()}";
        }
    }
}
