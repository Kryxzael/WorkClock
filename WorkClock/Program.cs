using System;
using System.Collections;
using System.Linq;
using System.Threading;

namespace WorkClock
{
    class Program
    {
        public static void Main(string[] args)
        {
            for (int i = 0; i < args.Length; i++)
            {
                string subArg = null;
                string arg = args[i];

                if (i < args.Length - 1 && !args[i + 1].StartsWith("-"))
                    subArg = args[i + 1];

                if (arg.StartsWith("-") && !arg.StartsWith("--"))
                {
                    foreach (char c in arg.Skip(1))
                    {
                        parseArg("-" + c, null);
                    }
                }
                else
                {
                    parseArg(arg, subArg);
                }
            }

            Pump();

            void parseArg(string arg, string subArg)
            {
                switch (arg)
                {
                    case "--adjust" or "--adj" or "-a":
                        Data.AdjustEndTime = true;
                        break;

                    case "--late":
                        if (subArg != null && float.TryParse(subArg, out float lateOffset))
                        {
                            Data.TodayOffset = TimeSpan.FromHours(lateOffset);
                        }
                        else
                        {
                            goto default;
                        }

                        break;

                    case "--early":
                        if (subArg != null && float.TryParse(subArg, out float earlyOffset))
                        {
                            Data.TodayOffset = TimeSpan.FromHours(-earlyOffset);
                        }
                        else
                        {
                            goto default;
                        }

                        break;

                    case "-l":
                        Data.TodayOffset = TimeSpan.FromHours(0.5);
                        break;

                    case "-L":
                        Data.TodayOffset = TimeSpan.FromHours(1.0);
                        break;

                    case "-e":
                        Data.TodayOffset = TimeSpan.FromHours(-0.5);
                        break;

                    case "-E":
                        Data.TodayOffset = TimeSpan.FromHours(-1.0);
                        break;

                    case "--digits" or "-d":
                        Data.HumanReadableTimes = false;
                        break;

                    case "--help" or "-h" or "-?":
                    default:
                        Console.WriteLine("TODO: Syntax");
                        return;
                }
            }
        }

        private static void Pump()
        {
            int bufferWidth = 0, bufferHeight = 0;
            int lastRefreshedMinute = -1;

            do
            {
                if (bufferWidth != Console.BufferWidth || bufferHeight != Console.BufferHeight || DateTime.Now.Minute != lastRefreshedMinute)
                {
                    Console.Clear();
                    bufferWidth = Console.BufferWidth;
                    bufferHeight = Console.BufferHeight;
                    lastRefreshedMinute = DateTime.Now.Minute;
                }

                Console.CursorLeft = 0;
                Console.CursorTop = 0;
                Console.CursorVisible = false;

                WriteCalendar();

                Console.WriteLine();
                Console.WriteLine();

                WriteTable();

                Thread.Sleep(500);

            } while (true);
        }

        private static void WriteTable()
        {
            CLUITable table = new CLUITable()
            {
                Spacing = 1,
                RightAlignColumns = new[] { false, true, true, false }
            };

            table.Add("Today", CLUI.Date(Data.Now));
            table.Add("Current Time", CLUI.Time(Data.Now));

            if (Data.Now.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday)
            {
                DurationProgressInfo weekendProgress = new DurationProgressInfo(
                        Start: Data.ThisWeek.Start.Date.AddDays(4) + Constants.DayEnd,
                        End:   Data.ThisWeek.Start.Date.AddDays(7) + Constants.DayStart
                    );

                table.Add("Next Workweek In", CLUI.Time(weekendProgress.GetTimeUntilEnd()), CLUI.PercentageAndBar(weekendProgress.GetCompletionPercentage(), false, false));
                table.Separator();
                table.Separator();
                table.Add(CLUI.Rainbow("Have a nice weekend!!"));

                table.Write();
                Thread.Sleep(500);
                return;
            }

            string lunchText;
            string lunchColor = "";

            if (Data.Now.TimeOfDay < Constants.LunchStart - Constants.LunchSoonSpan)
            {
                lunchText = "Before";
            }
            else if (Data.Now.TimeOfDay < Constants.LunchStart)
            {
                lunchText = "Soon";
                lunchColor = CLUI.DARK_YELLOW;
            }

            else if (Data.Now.TimeOfDay > Constants.LunchEnd)
            {
                lunchText = "After";
                lunchColor = CLUI.DARK_GRAY;
            }
            else
            {
                lunchText = CLUI.Blink("In Progress");
                lunchColor = CLUI.RED;
            }

            table.Add("Lunch", lunchColor + lunchText);

            if (DateTime.Now.TimeOfDay < Constants.LunchStart)
                table.Add("   In", lunchColor + CLUI.Time(Constants.LunchStart - DateTime.Now.TimeOfDay));

            table.Add("In Core Workhours", CLUI.YesNo(Data.Now.TimeOfDay > Constants.CoreWorkHoursStart && Data.Now.TimeOfDay < Constants.CoreWorkHoursEnd, "Yes", CLUI.DARK_GRAY + "No"));

            if (DateTime.Now.TimeOfDay > Constants.LunchEnd && DateTime.Now.TimeOfDay < Constants.CoreWorkHoursEnd)
                table.Add("           End In", CLUI.Time(Constants.CoreWorkHoursEnd - DateTime.Now.TimeOfDay));

            table.Separator();

            table.Add("Arrived At", CLUI.Time(Data.TodayStart, forceSimpleFormating: true));
            table.Add("Leave At",   CLUI.Time(Data.TodayEnd,   forceSimpleFormating: true));

            table.Separator();

            var today = Data.TodaySpan;
            TimeSpan sinceStart = today.GetTimeSinceStart();
            TimeSpan untilEnd = today.GetTimeUntilEnd();

            if (sinceStart < default(TimeSpan))
            {
                table.Add("Time Until Work", CLUI.Time(sinceStart));
            }
            else
            {
                table.Add(
                    "Time At Work",
                    CLUI.Time(sinceStart),
                    CLUI.PercentageAndBar(today.GetCompletionPercentage(), false, false),
                    CLUI.RegisterableHoursWorked()
                );
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

                table.Add(
                    "Time Left", 
                    untilEndText, 
                    CLUI.PercentageAndBar(1f - today.GetCompletionPercentage(), false, true),
                    CLUI.RegisterableHoursLeft()
                );
            }
            else
            {
                table.Add("Overtime Worked", CLUI.GREEN + CLUI.Time(untilEnd));
            }


            table.Separator();

            TimeSpan timeThisWeek     = Data.GetDurationCompletedThisWeek();
            TimeSpan timeLeftThisWeek = Data.GetTotalDurationThisWeek() - timeThisWeek;
            float    weekPercentage   = (float)timeThisWeek.Ticks / Data.GetTotalDurationThisWeek().Ticks;

            if (timeThisWeek > Data.GetTotalDurationThisWeek())
                timeThisWeek = Data.GetTotalDurationThisWeek();

            if (timeLeftThisWeek.Ticks < 0)
                timeLeftThisWeek = default;


            table.Add("Time This Week", CLUI.Time(timeThisWeek), CLUI.PercentageAndBar(weekPercentage, true, false));
            table.Add("Time Left This Week", CLUI.Time(timeLeftThisWeek), CLUI.PercentageAndBar(1f - weekPercentage, true, true));

            table.Write();
        }
    
        private static void WriteCalendar()
        {
            int dayWidth = Console.BufferWidth / Constants.WeekLength;

            for (TimeSpan time = Constants.DayStart; time < Constants.DayEnd; time += new TimeSpan(1, 0, 0))
            {
                for (DateTime day = Data.ThisWeek.Start.Date; day <= Data.ThisWeek.End.Date; day = day.AddDays(1))
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
        }
    }
}
