using System;
using System.Threading;

namespace WorkClock
{
    class Program
    {
        public static void Main(string[] args)
        {
            int bufferWidth = 0, bufferHeight = 0;

            do
            {
                if (bufferWidth != Console.BufferWidth || bufferHeight != Console.BufferHeight)
                {
                    Console.Clear();
                    bufferWidth  = Console.BufferWidth;
                    bufferHeight = Console.BufferHeight;
                }

                Console.CursorLeft = 0;
                Console.CursorTop  = 0;
                Console.CursorVisible = false;

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

                            new CLUIBar(progress, (int)Math.Floor(sectionWidth))
                            {
                                EndCap = '|'
                            }.Write();

                            new CLUIBar(lunchProgress, (int)Math.Ceiling(sectionWidth) - 1)
                            {
                                FillPattern = "@",
                                StartCap = '|'
                            }.Write();

                            Console.Write(" ");
                        }
                        else
                        {
                            var progress = new DurationProgressInfo(day + time, new TimeSpan(1, 0, 0));

                            new CLUIBar(progress, dayWidth - 1)
                                .Write();

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
                    DurationProgressInfo weekendProgress = new DurationProgressInfo(
                        Start: Constants.ThisWeek.Start.Date.AddDays(4) + Constants.DayEnd,
                        End:   Constants.ThisWeek.Start.Date.AddDays(7) + Constants.DayStart
                    );

                    table.Add("Next Workweek In", CLUI.Time(weekendProgress.GetTimeUntilEnd()), CLUI.PercentageAndBar(weekendProgress.GetCompletionPercentage(), false, false));
                    table.Separator();
                    table.Separator();
                    table.Add("Have a nice weekend!!");

                    table.Write();
                    Thread.Sleep(500);
                    continue;
                }

                string lunchText;

                if (Constants.Now.TimeOfDay < Constants.LunchStart - Constants.LunchSoonSpan)
                    lunchText = "Before";

                else if (Constants.Now.TimeOfDay < Constants.LunchStart)
                    lunchText = CLUI.DARK_YELLOW + "Soon";

                else if (Constants.Now.TimeOfDay > Constants.LunchEnd)
                    lunchText = "After";

                else
                    lunchText = CLUI.Blink(CLUI.RED + "In Progress");

                table.Add("Lunch", lunchText);

                table.Add("In Core Workhours", CLUI.YesNo(Constants.Now.TimeOfDay > Constants.CoreWorkHoursStart && Constants.Now.TimeOfDay < Constants.CoreWorkHoursEnd));

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
                    table.Add("Time At Work", CLUI.Time(sinceStart), CLUI.PercentageAndBar(today.GetCompletionPercentage(), false, false));
                }

                if (untilEnd > default(TimeSpan))
                {
                    string untilEndText = "";

                    if (untilEnd < Constants.DayEndSoonRed)
                        untilEndText += CLUI.RED;

                    else if (untilEnd < Constants.DayEndSoonYellow)
                        untilEndText += CLUI.DARK_YELLOW;

                    untilEndText += CLUI.Time(untilEnd);

                    if (untilEnd < Constants.DayEndSoonBlink)
                        untilEndText = CLUI.Blink(untilEndText);

                    table.Add("Time Left", untilEndText, CLUI.PercentageAndBar(1f - today.GetCompletionPercentage(), false, true));
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


                table.Add("Time This Week",      CLUI.Time(timeThisWeek),     CLUI.PercentageAndBar(     weekPercentage, true, false));
                table.Add("Time Left This Week", CLUI.Time(timeLeftThisWeek), CLUI.PercentageAndBar(1f - weekPercentage, true, true));

                table.Write();
                Thread.Sleep(500);

            } while (true);
        }
    }
}
