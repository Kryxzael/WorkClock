using System;
using System.Collections.Generic;
using System.Drawing;
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

        public delegate (ConsoleColor, char) GetRenderedDataForPosition(float startValue, float endValue, int index);

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
        /// Gets the delegate that is used to get data to fill the bar's filled section with
        /// </summary>
        public GetRenderedDataForPosition GetFillData { get; init; }

        /// <summary>
        /// Gets the delegate that is used to get data to fill the bar's empty section with
        /// </summary>
        public GetRenderedDataForPosition GetEmptyData { get; init; }

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

            float percentPerSegment = 1f / ContentLength;
            ConsoleColor lastColor = CapColor;

            for (int i = (RightAlign ? ContentLength - 1 : 0); (RightAlign ? i >= 0 : i < ContentLength); i += (RightAlign ? -1 : 1))
            {
                float progressMin = percentPerSegment * i;
                float progressMax = percentPerSegment * (i + 1);

                ConsoleColor color;
                char fillChar;

                if (progressMax <= Value)
                    (color, fillChar) = GetFillData(progressMin, progressMax, i);

                else
                    (color, fillChar) = GetEmptyData(progressMin, progressMax, i);

                if (lastColor != color)
                {
                    output.Append(CLUI.EncodeColor(color));
                    lastColor = color;
                }

                output.Append(fillChar);
            }

            output.Append(CLUI.EncodeColor(CapColor));
            output.Append(EndCap);

            return output.ToString();
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
