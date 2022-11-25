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
    public class CLUISimpleBar : CLUIBar
    {
        /// <summary>
        /// Gets or sets the pattern to fill the bar with
        /// </summary>
        public string FillPattern { get; set; } = "#";

        /// <summary>
        /// Gets or sets the pattern to fill the bar's background with
        /// </summary>
        public string BackgroundPattern { get; set; } = " ";

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
        public CLUISimpleBar(float value, int totalLength) : base(value, totalLength)
        {
            GetFillData  = (_, _, index) => (FillColor,       FillPattern[      index % FillPattern.Length]);
            GetEmptyData = (_, _, index) => (BackgroundColor, BackgroundPattern[index % BackgroundPattern.Length]);
        }

        /// <summary>
        /// Creates a new bar from a duration progress info object
        /// </summary>
        /// <param name="source"></param>
        /// <param name="totalLength"></param>
        public CLUISimpleBar(DurationProgressInfo source, int totalLength) : this(source.GetCompletionPercentage(), totalLength)
        {  }
    }
}
