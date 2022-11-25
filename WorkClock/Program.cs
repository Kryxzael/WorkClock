using System;
using System.Collections;
using System.Globalization;
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
                        if (subArg != null && float.TryParse(subArg, NumberStyles.Number, CultureInfo.InvariantCulture, out float lateOffset))
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
                table.Add("Overtime Worked", CLUI.GREEN + CLUI.Time(untilEnd), "", CLUI.GREEN + CLUI.RegisterableHoursWorkedOvertime());
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
            int dayWidth = (Console.BufferWidth - 3) / Constants.WeekLength;

            Console.Write("    ");
            foreach (string i in new[] { "Monday", "Tuesday", "Wednesday", "Thursday", "Friday" })
            {
                if (i.Length > dayWidth)
                    continue;

                if (Data.Now.DayOfWeek.ToString() == i)
                {
                    Console.BackgroundColor = ConsoleColor.Cyan;
                    Console.ForegroundColor = ConsoleColor.Black;
                }

                Console.Write(i);

                Console.ForegroundColor = ConsoleColor.Gray;
                Console.BackgroundColor = ConsoleColor.Black;

                Console.Write(new string(' ', dayWidth - i.Length));
            }

            Console.WriteLine();

            for (
                TimeSpan time = Constants.DayStart; 
                time.Hours < new[]{ Constants.DayEnd.Hours, Data.Now.Hour + 1, Data.TodayEnd.Hours }.Max(); 
                time += new TimeSpan(1, 0, 0)
            )
            {
                if (Data.Now.Hour == time.Hours)
                {
                    if (time.Hours >= Constants.DayEnd.Hours)
                        Console.BackgroundColor = ConsoleColor.DarkYellow;
                    else
                        Console.BackgroundColor = ConsoleColor.Cyan;
                    Console.ForegroundColor = ConsoleColor.Black;
                }

                else if (time.Hours >= Constants.DayEnd.Hours)
                    Console.ForegroundColor = ConsoleColor.DarkYellow;

                Console.Write(time.Hours.ToString("00"));

                Console.ForegroundColor = ConsoleColor.Gray;
                Console.BackgroundColor = ConsoleColor.Black;

                Console.Write(" ");

                for (DateTime day = Data.ThisWeek.Start.Date; day <= Data.ThisWeek.End.Date; day = day.AddDays(1))
                {
                    if (time >= Constants.DayEnd && day != Data.Now.Date)
                    {
                        Console.Write(new string(' ', dayWidth));
                        continue;
                    }

                    var progress = new DurationProgressInfo(day + time, new TimeSpan(1, 0, 0));

                    if (time.Hours == 11)
                    {
                        new CLUIBar(progress, dayWidth - 1)
                        {
                            GetFillData = (min, _, _) => min < 0.5f ? (ConsoleColor.Gray, '#') : (ConsoleColor.DarkGray, '@'),
                            GetEmptyData = (_, _, _) => (ConsoleColor.Gray, ' ')
                        }.Write();

                        Console.Write(" ");
                    }
                    else
                    {
                        new CLUIBar(progress, dayWidth - 1)
                        {
                            GetFillData = (_, max, _) => {
                                //Skipping first hours (late)
                                if (time + TimeSpan.FromHours(max) <= Data.TodayStart && day == Data.Now.Date)
                                    return (ConsoleColor.DarkRed, 'X');

                                //Working overtime
                                else if (time + TimeSpan.FromHours(max) > Data.TodayEnd && day == Data.Now.Date)
                                    return (ConsoleColor.Green, '+');

                                return (ConsoleColor.Gray, '#');
                            },
                            GetEmptyData = (_, _, _) => (ConsoleColor.Gray, ' ')
                        }.Write();

                        Console.Write(" ");
                    }
                }

                Console.WriteLine();
            }
        }
    }
}
