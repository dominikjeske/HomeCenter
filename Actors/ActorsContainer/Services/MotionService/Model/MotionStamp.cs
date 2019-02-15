using System;

namespace HomeCenter.Services.MotionService.Model
{
    public class MotionStamp
    {
        public DateTimeOffset? Time { get; private set; }
        public bool CanConfuze { get; private set; }
        public MotionStamp Previous { get; private set; }

        public override string ToString() => $"{(Time != null ? Time?.Second.ToString() : "?")}:{(Time != null ? Time?.Millisecond.ToString() : "?")}";

        public void SetTime(DateTimeOffset time)
        {
            if (Time.HasValue)
            {
                Previous = Clone();
            }

            Time = time;
            CanConfuze = true;
        }

        public void UnConfuze() => CanConfuze = false;

        public MotionStamp Clone() => (MotionStamp)MemberwiseClone();
    }
}