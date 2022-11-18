using System;
using System.Threading;

namespace WorkClock
{
    class Program
    {
        public static void Main(string[] args)
        {
            do
            {
                Console.Clear();

                int dayWidth = Console.BufferWidth / Constants.WeekLength;

                for (TimeSpan time = Constants.DayStart; time < Constants.DayEnd; time += new TimeSpan(1, 0, 0))
                {
                    for (DateTime day = Constants.ThisWeek.Start.Date; day <= Constants.ThisWeek.End.Date; day = day.AddDays(1))
                    {
                        if (time.Hours == 11)
                        {
                            var progress      = new Constants.DurationProgressInfo(day + time,  new TimeSpan(0, 30, 0));
                            var lunchProgress = new Constants.DurationProgressInfo(day + time + new TimeSpan(0, 30, 0), new TimeSpan(0, 30, 0));
                            float sectionWidth  = dayWidth / 2f;


                            Console.Write(progress.CreateBar(     (int)Math.Floor(sectionWidth),       end:   '|'));
                            Console.Write(lunchProgress.CreateBar((int)Math.Ceiling(sectionWidth) - 1, start: '|', fill: '@'));
                            Console.Write(" ");
                        }
                        else
                        {
                            var progress = new Constants.DurationProgressInfo(day + time, new TimeSpan(1, 0, 0));

                            Console.Write(progress.CreateBar(dayWidth - 1));
                            Console.Write(" ");
                        }
                    }

                    Console.WriteLine();
                }

                Console.WriteLine();
                Console.WriteLine();
                Console.WriteLine(Constants.CreateTableEntry("Today", Constants.Now.ToString("dddd MMM-d")));
                Console.WriteLine(Constants.CreateTableEntry("Current Time", Constants.Now.ToString("HH:mm:ss")));

                if (Constants.Now.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday)
                {
                    Console.WriteLine(Constants.CreateTableEntry("Next Workweek In", Constants.ThisWeek.Start.AddDays(7) - Constants.Now));
                    Console.WriteLine();
                    Console.WriteLine();
                    Console.WriteLine("Have a nice weekend!!");
                    Thread.Sleep(500);
                    continue;
                }

                string lunchText;

                if (Constants.Now.TimeOfDay < new TimeSpan(11, 20, 00))
                {
                    lunchText = "Before";
                }
                else if (Constants.Now.TimeOfDay < new TimeSpan(11, 30, 00))
                {
                    lunchText = "Soon";
                    Console.ForegroundColor = ConsoleColor.DarkYellow;
                }
                else if (Constants.Now.TimeOfDay > new TimeSpan(12, 00, 00))
                {
                    lunchText = "After";
                }
                else
                {
                    if (DateTime.Now.Millisecond < 500)
                        lunchText = "In Progress";
                    else
                        lunchText = "";
                    Console.ForegroundColor = ConsoleColor.Red;
                }

                Console.WriteLine(Constants.CreateTableEntry("Lunch", lunchText));
                Console.ForegroundColor = ConsoleColor.Gray;

                string coreWorkhoursText = "Unknown";

                if (Constants.Now.TimeOfDay > new TimeSpan(9, 0, 0) && Constants.Now.TimeOfDay < new TimeSpan(14, 0, 0))
                    coreWorkhoursText = "Yes";

                else
                    coreWorkhoursText = "No";

                Console.WriteLine(Constants.CreateTableEntry("In Core Workhours", coreWorkhoursText));

                Console.WriteLine();

                var today = Constants.GetDaySpan(Constants.Now);
                TimeSpan sinceStart = today.GetTimeSinceStart();
                TimeSpan untilEnd = today.GetTimeUntilEnd();

                if (sinceStart < default(TimeSpan))
                {
                    Console.WriteLine(Constants.CreateTableEntry("Time Until Work", sinceStart));
                }
                else
                {
                    Console.Write(Constants.CreateTableEntry("Time At Work", sinceStart));
                    Console.Write("  ");
                    Console.WriteLine((today.GetCompletionPercentage() * 100).ToString("0").PadLeft(3) + "% Complete");
                }

                if (untilEnd > default(TimeSpan))
                {
                    if (untilEnd < new TimeSpan(0, 15, 0))
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                    }
                    else if (untilEnd < new TimeSpan(1, 0, 0))
                    {
                        Console.ForegroundColor = ConsoleColor.DarkYellow;
                    }

                    if (untilEnd < new TimeSpan(0, 5, 0) && DateTime.Now.Millisecond < 500)
                        Console.Write(Constants.CreateTableEntry("Time Left", ""));
                    else
                        Console.Write(Constants.CreateTableEntry("Time Left", untilEnd));

                    Console.Write("  ");
                    Console.WriteLine(((1f - today.GetCompletionPercentage()) * 100).ToString("0").PadLeft(3) + "% Left");

                    Console.ForegroundColor = ConsoleColor.Gray;
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine(Constants.CreateTableEntry("Overtime Worked", untilEnd));
                    Console.ForegroundColor = ConsoleColor.Gray;
                }

                Console.WriteLine();

                TimeSpan timeThisWeek     = Constants.GetDurationCompletedThisWeek();
                TimeSpan timeLeftThisWeek = Constants.GetTotalDurationThisWeek() - timeThisWeek;
                float    weekPercentage   = (float)timeThisWeek.Ticks / Constants.GetTotalDurationThisWeek().Ticks;

                if (timeThisWeek > Constants.GetTotalDurationThisWeek())
                    timeThisWeek = Constants.GetTotalDurationThisWeek();

                if (timeLeftThisWeek.Ticks < 0)
                    timeLeftThisWeek = default;

                weekPercentage = Math.Clamp(weekPercentage, 0f, 1f);

                Console.Write(Constants.CreateTableEntry("Time This Week", timeThisWeek));
                Console.Write("  ");
                Console.WriteLine((weekPercentage * 100).ToString("0").PadLeft(3) + "% Complete");

                Console.Write(Constants.CreateTableEntry("Time Left This Week", timeLeftThisWeek));
                Console.Write("  ");
                Console.WriteLine(((1f - weekPercentage) * 100).ToString("0").PadLeft(3) + "% Left");

                Thread.Sleep(500);

            } while (true);
        }
    }
}
