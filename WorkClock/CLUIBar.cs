using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorkClock
{
    /// <summary>
    /// Renders a progress bar to the command line interface
    /// </summary>
    public class CLUIBar
    {
        private float _value;
        private int   _length;

        /// <summary>
        /// Gets or sets the full length of the bar, including the caps
        /// </summary>
        public int TotalLength
        {
            get => _length;
            set
            {
                if (value < 3)
                    throw new ArgumentOutOfRangeException(nameof(value), "TotalLength must be at least 3 to account for cap sizes");

                _length = value;
            }
        }

        /// <summary>
        /// Gets or sets the length of the contents of the bar, not including the caps.
        /// </summary>
        public int ContentLength
        {
            get => _length - 2;
            set
            {
                if (_length <= 0)
                    throw new ArgumentOutOfRangeException(nameof(value), "ContentLength must be a positive number");

                _length = value + 2;
            }
        }

        /// <summary>
        /// Gets or sets the current value of the bar
        /// </summary>
        public float Value
        {
            get => _value;
            set => _value = Math.Clamp(value, 0f, 1f);
        }

        /// <summary>
        /// Gets whether the fill of the bar should float to the right
        /// </summary>
        public bool RightAlign { get; set; }

        /// <summary>
        /// Gets or sets the pattern to fill the bar with
        /// </summary>
        public string FillPattern { get; set; } = "#";

        /// <summary>
        /// Gets or sets the pattern to fill the bar's background with
        /// </summary>
        public string BackgroundPattern { get; set; } = " ";

        /// <summary>
        /// Gets or sets the cap marking the start of the bar
        /// </summary>
        public char StartCap { get; set; } = '[';

        /// <summary>
        /// Gets or sets the cap marking the end of the bar
        /// </summary>
        public char EndCap { get; set; } = ']';

        /// <summary>
        /// Gets or sets the color to use at the caps of the bar
        /// </summary>
        public ConsoleColor CapColor { get; set; } = ConsoleColor.Gray;

        /// <summary>
        /// Gets or sets the color to use for the background of the bar
        /// </summary>
        public ConsoleColor BackgroundColor { get; set; } = ConsoleColor.Gray;

        /// <summary>
        /// Gets or sets the color to use for the fill of the bar
        /// </summary>
        public ConsoleColor FillColor { get; set; } = ConsoleColor.Gray;

        /// <summary>
        /// Creates a new bar
        /// </summary>
        /// <param name="value"></param>
        /// <param name="totalLength"></param>
        public CLUIBar(float value, int totalLength)
        {
            Value = value;
            TotalLength = totalLength;
        }

        /// <summary>
        /// Creates a new bar from a duration progress info object
        /// </summary>
        /// <param name="source"></param>
        /// <param name="totalLength"></param>
        public CLUIBar(DurationProgressInfo source, int totalLength) : this(source.GetCompletionPercentage(), totalLength)
        {  }

        /// <summary>
        /// Renders the bar's contents as a string, optionally with color encoding
        /// </summary>
        /// <returns></returns>
        public string RenderString()
        {
            StringBuilder output = new StringBuilder();

            int fillLength = (int)Math.Round(Value * ContentLength);
            int emptyLength = ContentLength - fillLength;

            output.Append(CLUI.EncodeColor(CapColor));
            output.Append(StartCap);

            if (!RightAlign)
            {
                output.Append(CLUI.EncodeColor(FillColor));
                output.Append(repeat(FillPattern, fillLength, 0));

                output.Append(CLUI.EncodeColor(BackgroundColor));
                output.Append(repeat(BackgroundPattern, emptyLength, fillLength));
            }
            else
            {
                output.Append(CLUI.EncodeColor(BackgroundColor));
                output.Append(repeat(BackgroundPattern, emptyLength, fillLength));

                output.Append(CLUI.EncodeColor(FillColor));
                output.Append(repeat(FillPattern, fillLength, 0));
            }

            output.Append(CLUI.EncodeColor(CapColor));
            output.Append(EndCap);

            return output.ToString();

            static string repeat(string str, int length, int offset)
            {
                string output = "";

                for (int i = 0; i < length; i++)
                    output += str[(i + offset) % str.Length];

                return output;
            }
        }

        /// <summary>
        /// Renders the bar's contents to the CLI
        /// </summary>
        public void Write()
        {
            CLUI.Write(RenderString());
        }

        /// <summary>
        /// Renders the bar's contents to the CLI
        /// </summary>
        public void WriteLine()
        {
            CLUI.WriteLine(RenderString());
        }
    }
}
