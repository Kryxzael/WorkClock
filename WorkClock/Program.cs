﻿using System;
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
            Console.CancelKeyPress += (s, e) =>
            {
                Console.CursorVisible = true;
                Console.Clear();
                Environment.Exit(0); //Required due to application soft-lock if we are in the middle of a Thread.Sleep call when sending interrupt
            };

            for (int i = 0; i < args.Length; i++)
            {
                string subArg = null;
                string arg = args[i];

                if (i < args.Length - 1 && !args[i + 1].StartsWith("-"))
                    subArg = args[i + 1];

                bool subArgProcessed = false;

                if (arg.StartsWith("-") && !arg.StartsWith("--"))
                {
                    foreach (char c in arg.Skip(1))
                    {
                        parseArg("-" + c, null, out subArgProcessed);
                    }
                }
                else
                {
                    parseArg(arg, subArg, out subArgProcessed);
                }

                if (subArgProcessed)
                    i++;
            }

            try
            {
                Pump();
            }
            finally
            {
                Console.CursorVisible = true;
            }

            void parseArg(string arg, string subArg, out bool subArgProcessed)
            {
                subArgProcessed = false;

                switch (arg)
                {
                    case "--adjust" or "--adj" or "-a":
                        Data.AdjustEndTime = true;
                        break;

                    case "--late":
                        if (subArg != null && float.TryParse(subArg, NumberStyles.Number, CultureInfo.InvariantCulture, out float lateOffset))
                        {
                            Data.TodayOffset = TimeSpan.FromHours(lateOffset);
                            subArgProcessed = true;
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
                        WriteSyntax();
                        Environment.Exit(0);
                        return;

                    default:
                        if (Meeting.TryParse(arg, out Meeting result))
                        {
                            Data.Meetings.Add(result);
                        }
                        else
                        {
                            WriteSyntax();
                            Environment.Exit(1);
                        }
                        return;
                }
            }
        }

        private static void Pump()
        {
            int bufferWidth = 0, bufferHeight = 0;
            int lastRefreshedMinute = -1;

            while (true)
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
            }
        }

        private static void WriteSyntax()
        {
            Console.WriteLine("Usage: workclock.exe [--adjust] [--late <hours> | --early <hours>] [--digits] [<Meetings>]");
            Console.WriteLine();

            CLUITable switches = new CLUITable { Spacing = 2 };
            switches.Add("--adj[ust]",      "-a",    "Adjusts the end-time 15 minutes ahead");
            switches.Add("--late <hours>",  "",      "Offsets the arrival time some time into the future");
            switches.Add("",                "-L",    "Offsets the arrival time 1 hour into the future. Can be repeated");
            switches.Add("",                "-l",    "Offsets the arrival time half-an-hour into the future. Can be repeated");
            switches.Add("--early <hours>", "",      "Offsets the arrival time some time into the past");
            switches.Add("",                "-E",    "Offsets the arrival time 1 hour into the past. Can be repeated");
            switches.Add("",                "-e",    "Offsets the arrival time half-an-hour into the past. Can be repeated");
            switches.Add("--digits",        "-d",    "Uses digital clocks for countdowns/stopwatches, rather than human-friendly values");
            switches.Add("--help",          "-h -?", "Shows this help page");
            switches.WriteLine();
            Console.WriteLine();

            Console.WriteLine("Meeting syntax:");
            Console.WriteLine("Meetings can be specified for any day and any time of the current week. You may add as many meetings as you like");
            Console.WriteLine();

            CLUITable meetings = new CLUITable { Spacing = 5 };
            meetings.Add("1:00pm", "Meeting today at 1 PM (13:00). Lasting one hour by default");
            meetings.Add("1pm",    "Meeting today at 1 PM (13:00). Lasting one hour by default");
            meetings.Add("13:00",  "Meeting today at 1 PM (13:00). [...]");
            meetings.Add("13",     "Meeting today at 1 PM (13:00)");
            meetings.Separator();
            meetings.Add("monday 16:30",   "Meeting on Monday at 4:30 PM (16:00)");
            meetings.Add("mon 16:30",      "Meeting on Monday at 4:30 PM (16:00)");
            meetings.Add("tomorrow 16:30", "Meeting the following day at 4:30 PM (16:00)");
            meetings.Add("friday noon",    "Meeting on Friday at 12 PM (12:00)");
            meetings.Separator();
            meetings.Add("1pm-2pm", "Meeting today starting 1 PM (14:00) and ending 2 PM (14:00)");
            meetings.Add("1pm-2h",  "Meeting today starting 1 PM (14:00) and lasting 2 hours");
            meetings.Add("1pm-2.0", "Meeting today starting 1 PM (14:00) and lasting 2 hours");
            meetings.Add("1pm-2",   "Meeting today starting 1 PM (14:00) and " + CLUI.DARK_YELLOW + "ending at 2 AM (02:00)");
            meetings.Add("1pm-2.5", "Meeting today starting 1 PM (14:00) and lasting 2 hours and 30 minutes");
            meetings.Add("thursday start-end", "Meeting on Thursday spanning the entire workday");
            meetings.Add("thursday 8-lunch", "Meeting on Thursday starting at 8 AM (08:00) and lasting until lunch");

            meetings.WriteLine();
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

            Meeting currentOrNextMeeting = Data.Meetings.FirstOrDefault(i => i.EndTime > Data.Now);

            if (currentOrNextMeeting != null && currentOrNextMeeting.StartTime.Date == Data.Now.Date)
            {
                DurationProgressInfo progress = currentOrNextMeeting.GetProgress();

                if (progress.GetTimeSinceStart() > default(TimeSpan))
                {
                    table.Add("Meeting Ends At", CLUI.CYAN + CLUI.Time(currentOrNextMeeting.EndTime));
                    table.Add("Meeting Ends In", CLUI.CYAN + CLUI.Time(progress.GetTimeUntilEnd()), CLUI.PercentageAndBar(progress.GetCompletionPercentage(), false, false, ">"));
                }
                else
                {
                    string color = CLUI.GRAY;

                    if (progress.GetTimeSinceStart() >= -Constants.MeetingSoonRed)
                        color = CLUI.RED;

                    else if (progress.GetTimeSinceStart() >= -Constants.MeetingSoonYellow)
                        color = CLUI.DARK_YELLOW;

                    table.Add("Next Meeting At", CLUI.Time(currentOrNextMeeting.StartTime));

                    if (progress.GetTimeSinceStart() >= -Constants.MeetingSoonBlink)
                        table.Add("Next Meeting In", color + CLUI.Blink(CLUI.Time(-progress.GetTimeSinceStart())));

                    else
                        table.Add("Next Meeting In", color + CLUI.Time(-progress.GetTimeSinceStart()));

                }

                table.Separator();
            }


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
            foreach (DayOfWeek i in Enumerable.Range((int)DayOfWeek.Monday, (int)DayOfWeek.Friday).Cast<DayOfWeek>())
            {
                string dayString = i.ToString() + " " + Data.GetDateAtWeekDay(i).Day.ToString(CultureInfo.InvariantCulture);

                if (dayString.Length > dayWidth)
                    continue;

                if (Data.Now.DayOfWeek == i)
                {
                    Console.BackgroundColor = ConsoleColor.Cyan;
                    Console.ForegroundColor = ConsoleColor.Black;
                }

                Console.Write(dayString);

                Console.ForegroundColor = ConsoleColor.Gray;
                Console.BackgroundColor = ConsoleColor.Black;

                Console.Write(new string(' ', dayWidth - dayString.Length));
            }

            Console.WriteLine();

            double maxHour = Constants.DayEnd.TotalHours;
            maxHour = Math.Max(maxHour, Data.Now.Hour + Data.Now.Minute / 60.0);
            maxHour = Math.Max(maxHour, Data.TodayEnd.TotalHours);

            if (Data.Meetings.Any())
            {
                Meeting lastMeeting = Data.Meetings
                    .Where(i => i.EndTime.Date == Data.Now.Date)
                    .OrderBy(i => i.EndTime)
                    .Last();

                maxHour = Math.Max(maxHour, lastMeeting.EndTime.Hour + lastMeeting.EndTime.Minute / 60.0);
            }    

            for (TimeSpan time = Constants.DayStart; time.TotalHours < Math.Ceiling(maxHour);  time += new TimeSpan(1, 0, 0))
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

                    new CLUIBar(progress, dayWidth - 1)
                    {
                        GetFillData = (min, max, _) => {
                            //Skipping first hours (late)
                            if (time + TimeSpan.FromHours(max) <= Data.TodayStart && day == Data.Now.Date)
                                return (ConsoleColor.DarkRed, 'X');

                            //Meeting
                            if (Data.InMeeting(day + time + TimeSpan.FromHours(min), day + time + TimeSpan.FromHours(max)))
                            {
                                //Overtime meeting
                                if (time + TimeSpan.FromHours(max) > Data.TodayEnd && day == Data.Now.Date)
                                    return (ConsoleColor.Green, '>');

                                return (ConsoleColor.Cyan, '>');
                            }

                            //Lunch
                            if (time + TimeSpan.FromHours(min) >= new TimeSpan(11, 30, 0) && time + TimeSpan.FromHours(max) <= new TimeSpan(12, 0, 0))
                                return (ConsoleColor.DarkGray, '@');

                            //Working overtime
                            else if (time + TimeSpan.FromHours(max) > Data.TodayEnd && day == Data.Now.Date)
                                return (ConsoleColor.Green, '+');

                            return (ConsoleColor.Gray, '#');
                        },
                        GetEmptyData = (min, max, _) =>
                        {
                            if (Data.InMeeting(day + time + TimeSpan.FromHours(min), day + time + TimeSpan.FromHours(max)))
                                return (ConsoleColor.DarkGray, '.');

                            return (ConsoleColor.Gray, ' ');
                        }
                    }.Write();

                    Console.Write(" ");
                }

                Console.WriteLine();
            }
        }
    }
}
