using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorkClock
{
    public static class CLUI
    {
        private const char COLOR_CONTROL = '§';
        private static ConsoleColor[] RAINBOW_COLORS = { ConsoleColor.DarkRed, ConsoleColor.DarkYellow, ConsoleColor.Green, ConsoleColor.Blue, ConsoleColor.Cyan, ConsoleColor.Magenta };

        public static readonly string BLACK        = EncodeColor(ConsoleColor.Black);
        public static readonly string DARK_BLUE    = EncodeColor(ConsoleColor.DarkBlue);
        public static readonly string DARK_GREEN   = EncodeColor(ConsoleColor.DarkGreen);
        public static readonly string DARK_CYAN    = EncodeColor(ConsoleColor.DarkCyan);
        public static readonly string DARK_RED     = EncodeColor(ConsoleColor.DarkRed);
        public static readonly string DARK_MAGENTA = EncodeColor(ConsoleColor.DarkMagenta);
        public static readonly string DARK_YELLOW  = EncodeColor(ConsoleColor.DarkYellow);
        public static readonly string GRAY         = EncodeColor(ConsoleColor.Gray);
        public static readonly string DARK_GRAY    = EncodeColor(ConsoleColor.DarkGray);
        public static readonly string BLUE         = EncodeColor(ConsoleColor.Blue);
        public static readonly string GREEN        = EncodeColor(ConsoleColor.Green);
        public static readonly string CYAN         = EncodeColor(ConsoleColor.Cyan);
        public static readonly string RED          = EncodeColor(ConsoleColor.Red);
        public static readonly string MAGENTA      = EncodeColor(ConsoleColor.Magenta);
        public static readonly string WHITE        = EncodeColor(ConsoleColor.White);

        /// <summary>
        /// Creates a flashing label
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static string Blink(string text)
        {
            if (System.DateTime.Now.Millisecond < 500)
                return text;

            return new string(' ', EncodedStringLength(text));
        }

        /// <summary>
        /// Creates a label that changes color
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static string Rainbow(string text)
        {
            int colorIndex = (int)((DateTime.Now.Ticks / new TimeSpan(0, 0, 0, 0, 500).Ticks) % RAINBOW_COLORS.Length);

            return EncodeColor(RAINBOW_COLORS[colorIndex]) + text;
        }

        /// <summary>
        /// Formats the time-stamp of the provided date-time
        /// </summary>
        /// <param name="dt"></param>
        /// <returns></returns>
        public static string Time(DateTime dt)
        {
            return dt.ToString("HH:mm:ss");
        }

        /// <summary>
        /// Formats the provided time-span
        /// </summary>
        /// <param name="ts"></param>
        /// <returns></returns>
        public static string Time(TimeSpan ts, bool forceSimpleFormating = false)
        {
            if (ts < default(TimeSpan))
                ts = new TimeSpan(-ts.Ticks);

            if (forceSimpleFormating || !Data.HumanReadableTimes)
                return ((int)ts.TotalHours).ToString() + ts.ToString("':'mm':'ss");

            else if (ts.TotalSeconds < 1f)
                return "Zero";

            else if (ts.TotalMinutes < 1f)
                return writeComponent(ts.Seconds, "second", "seconds");

            else if (ts.TotalHours < 1f)
                return writeComponent(ts.Minutes, "minute, ", "minutes, ") + writeComponent(ts.Seconds, "sec", "sec");

            else
                return writeComponent((int)ts.TotalHours, "hour, ", "hours, ") + writeComponent(ts.Minutes, "min", "min");


            string writeComponent(int num, string suffixSingular, string suffixPlural)
            {
                int suffixLength = Math.Max(suffixSingular.Length, suffixPlural.Length);
                int numLength    = 2;

                if (num == 1)
                    return num.ToString().PadLeft(numLength) + " " + suffixSingular.PadRight(suffixLength);

                return num.ToString().PadLeft(numLength) + " " + suffixPlural.PadRight(suffixLength);
            }
        }

        /// <summary>
        /// Formats the date of the provided date-time
        /// </summary>
        /// <param name="dt"></param>
        /// <returns></returns>
        public static string Date(DateTime dt)
        {
            return dt.Date.ToString("dddd MMM d");
        }

        /// <summary>
        /// Formats the provided boolean with the provided yes/no values
        /// </summary>
        /// <param name="value"></param>
        /// <param name="yes"></param>
        /// <param name="no"></param>
        /// <returns></returns>
        public static string YesNo(bool value, string yes = "Yes", string no = "No")
        {
            if (value)
                return yes;

            return no;
        }

        /// <summary>
        /// Formats the provided percentage, optionally clamping it from 0 to 100
        /// </summary>
        /// <param name="percentage"></param>
        /// <param name="clamp"></param>
        /// <returns></returns>
        public static string Percentage(float percentage, bool clamp = false)
        {
            if (clamp)
                percentage = Math.Clamp(percentage, 0f, 1f);

            return ((int)(percentage * 100f)).ToString("0").PadLeft(3) + "%";
        }

        /// <summary>
        /// Formats the provided percentage and renders a progress bar next to it
        /// </summary>
        /// <param name="percentage"></param>
        /// <param name="clampPercentage"></param>
        /// <param name="rightAlignBar"></param>
        /// <returns></returns>
        public static string PercentageAndBar(float percentage, bool clampPercentage = false, bool rightAlignBar = false)
        {
            return Percentage(percentage, clampPercentage) + " " + new CLUIBar(percentage, 20) { RightAlign = rightAlignBar }.RenderString();
        }

        /// <summary>
        /// Writes the provided color encoded text to the console
        /// </summary>
        /// <param name="encodedText"></param>
        public static void Write(string encodedText)
        {
            string[] splitForColors = encodedText.Split(COLOR_CONTROL, StringSplitOptions.RemoveEmptyEntries);
            bool firstEntryHasColor = encodedText.StartsWith(COLOR_CONTROL);

            if (splitForColors.Length == 0)
                return;

            if (firstEntryHasColor)
            {
                Console.ForegroundColor = DecodeColor(COLOR_CONTROL + splitForColors[0]);
                Console.Write(splitForColors[0][1..]);
            }
            else
            {
                Console.Write(splitForColors[0]);
            }


            foreach (string i in splitForColors.Skip(1))
            {
                Console.ForegroundColor = DecodeColor(COLOR_CONTROL + i);
                Console.Write(i[1..]);
            }

            Console.ForegroundColor = ConsoleColor.Gray;
        }

        public static string RegisterableHoursWorked()
        {
            TimeSpan diff = Data.Now.TimeOfDay - Data.TodayStart;

            if (diff < default(TimeSpan))
                diff = default;

            if (Data.Now.TimeOfDay >= Constants.LunchEnd)
                diff -= TimeSpan.FromMinutes(30.0);

            return "+" + (Math.Round(diff.TotalHours / 0.5f) * 0.5f).ToString("0.0", CultureInfo.InvariantCulture) + "h";
        }

        public static string RegisterableHoursLeft()
        {
            TimeSpan diff = Data.TodayEndUnadjusted - Data.Now.TimeOfDay;

            if (diff < default(TimeSpan))
                diff = default;

            if (Data.Now.TimeOfDay < Constants.LunchEnd)
                diff -= TimeSpan.FromMinutes(30.0);

            return "-" + (Math.Round(diff.TotalHours / 0.5f) * 0.5f).ToString("0.0", CultureInfo.InvariantCulture) + "h";
        }

        /// <summary>
        /// Writes the provided color encoded text to the console, followed by a line-break
        /// </summary>
        /// <param name="encodedText"></param>
        public static void WriteLine(string encodedText)
        {
            Write(encodedText);
            Console.WriteLine();
        }

        /// <summary>
        /// Converts a console color into a string that can be decoded
        /// </summary>
        /// <param name="color"></param>
        /// <returns></returns>
        public static string EncodeColor(ConsoleColor color)
        {
            return COLOR_CONTROL + ((int)color).ToString("x");
        }

        /// <summary>
        /// Decodes an encoded color sequence
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        public static ConsoleColor DecodeColor(string code)
        {
            if (code.Length < 2)
                throw new ArgumentException("Invalid color code. Expected at least two chars in input string");

            if (code[0] != COLOR_CONTROL)
                throw new ArgumentException("Invalid color code. Provided argument is not a color code");

            return (ConsoleColor)int.Parse(code[1].ToString(), System.Globalization.NumberStyles.HexNumber);
        }

        /// <summary>
        /// Gets the length of a string when its encoding character sequences are removed
        /// </summary>
        /// <param name="encodedString"></param>
        /// <returns></returns>
        public static int EncodedStringLength(string encodedString)
        {
            return encodedString.Length - (encodedString.Count(i => i == COLOR_CONTROL) * 2);
        }
    }
}
