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
                            var progress      = new DurationProgressInfo(day + time,  new TimeSpan(0, 30, 0));
                            var lunchProgress = new DurationProgressInfo(day + time + new TimeSpan(0, 30, 0), new TimeSpan(0, 30, 0));
                            float sectionWidth  = dayWidth / 2f;


                            Console.Write(progress.CreateBar(     (int)Math.Floor(sectionWidth),       end:   '|'));
                            Console.Write(lunchProgress.CreateBar((int)Math.Ceiling(sectionWidth) - 1, start: '|', fill: '@'));
                            Console.Write(" ");
                        }
                        else
                        {
                            var progress = new DurationProgressInfo(day + time, new TimeSpan(1, 0, 0));

                            Console.Write(progress.CreateBar(dayWidth - 1));
                            Console.Write(" ");
                        }
                    }

                    Console.WriteLine();
                }

                Console.WriteLine();
                Console.WriteLine();

                CLUITable table = new CLUITable()
                {
                    Spacing = 1,
                    RightAlignColumns = new[] { false, true, true, false }
                };

                table.Add("Today",        CLUI.Date(Constants.Now));
                table.Add("Current Time", CLUI.Time(Constants.Now));

                if (Constants.Now.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday)
                {
                    table.Add("Next Workweek In", CLUI.Time(Constants.ThisWeek.Start.AddDays(7) - Constants.Now));
                    table.Separator();
                    table.Separator();
                    table.Add("Have a nice weekend!!");

                    table.Write();
                    Thread.Sleep(500);
                    continue;
                }

                string lunchText;

                if (Constants.Now.TimeOfDay < new TimeSpan(11, 20, 00))
                    lunchText = "Before";

                else if (Constants.Now.TimeOfDay < new TimeSpan(11, 30, 00))
                    lunchText = CLUI.DARK_YELLOW + "Soon";

                else if (Constants.Now.TimeOfDay > new TimeSpan(12, 00, 00))
                    lunchText = "After";

                else
                    lunchText = CLUI.Blink(CLUI.RED + "In Progress");

                table.Add("Lunch", lunchText);

                table.Add("In Core Workhours", CLUI.YesNo(Constants.Now.TimeOfDay > new TimeSpan(9, 0, 0) && Constants.Now.TimeOfDay < new TimeSpan(14, 0, 0)));

                table.Separator();

                var today = Constants.GetDaySpan(Constants.Now);
                TimeSpan sinceStart = today.GetTimeSinceStart();
                TimeSpan untilEnd = today.GetTimeUntilEnd();

                if (sinceStart < default(TimeSpan))
                {
                    table.Add("Time Until Work", CLUI.Time(sinceStart));
                }
                else
                {
                    table.Add("Time At Work", CLUI.Time(sinceStart), CLUI.Percentage(today.GetCompletionPercentage()), "Complete");
                }

                if (untilEnd > default(TimeSpan))
                {
                    string untilEndText = "";

                    if (untilEnd < new TimeSpan(0, 15, 0))
                        untilEndText += CLUI.RED;

                    else if (untilEnd < new TimeSpan(1, 0, 0))
                        untilEndText += CLUI.DARK_YELLOW;

                    untilEndText += CLUI.Time(untilEnd);

                    if (untilEnd.TotalMinutes < 5f)
                        untilEndText = CLUI.Blink(untilEndText);

                    table.Add("Time Left", untilEndText, CLUI.Percentage(1f - today.GetCompletionPercentage()), "Left");
                }
                else
                {
                    table.Add("Overtime Worked", CLUI.GREEN + CLUI.Time(untilEnd));
                }


                table.Separator();

                TimeSpan timeThisWeek     = Constants.GetDurationCompletedThisWeek();
                TimeSpan timeLeftThisWeek = Constants.GetTotalDurationThisWeek() - timeThisWeek;
                float    weekPercentage   = (float)timeThisWeek.Ticks / Constants.GetTotalDurationThisWeek().Ticks;

                if (timeThisWeek > Constants.GetTotalDurationThisWeek())
                    timeThisWeek = Constants.GetTotalDurationThisWeek();

                if (timeLeftThisWeek.Ticks < 0)
                    timeLeftThisWeek = default;


                table.Add("Time This Week",      CLUI.Time(timeThisWeek),     CLUI.Percentage(     weekPercentage, true), "Complete");
                table.Add("Time Left This Week", CLUI.Time(timeLeftThisWeek), CLUI.Percentage(1f - weekPercentage, true), "Left");

                table.Write();
                Thread.Sleep(500);

            } while (true);
        }
    }
}
