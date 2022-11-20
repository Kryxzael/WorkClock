using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorkClock
{
    public record DurationProgressInfo(DateTime Start, DateTime End)
    {
        public DurationProgressInfo(DateTime start, TimeSpan duration) : this(start, start + duration)
        { }

        public TimeSpan GetTimeSinceStart()
        {
            return Constants.Now - Start;
        }

        public TimeSpan GetTimeUntilEnd()
        {
            return End - Constants.Now;
        }

        public float GetCompletionPercentage()
        {
            return (Constants.Now.Ticks - Start.Ticks) / (float)(End - Start).Ticks;
        }
    }
}
