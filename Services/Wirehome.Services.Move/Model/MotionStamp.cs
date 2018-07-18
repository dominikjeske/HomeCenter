using Force.DeepCloner;
using System;

namespace Wirehome.Motion
{
    public class MotionStamp
    {
        public DateTimeOffset? Time { get; private set; }
        public bool CanConfuze { get; private set; }
        public MotionStamp Previous { get; private set; }

        public override string ToString() => $"{(Time != null ? Time?.Second.ToString() : "?")}:{(Time != null ? Time?.Millisecond.ToString() : "?")}";

        public void SetTime(DateTimeOffset time)
        {
            if(Time.HasValue)
            {
                Previous = this.ShallowClone();
            }

            Time = time;
            CanConfuze = true;
        }

        public void UnConfuze() => CanConfuze = false;
    }
}