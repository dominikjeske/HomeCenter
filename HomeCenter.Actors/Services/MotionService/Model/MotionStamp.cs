using System;

namespace HomeCenter.Services.MotionService.Model
{
    /// <summary>
    /// Represent motion time in room
    /// </summary>
    public class MotionStamp
    {
        public readonly static MotionStamp Empty = new MotionStamp();

        private MotionStamp _previous = Empty;

        public DateTimeOffset? Time { get; private set; }
        public MotionStamp Previous => _previous ?? Empty;

        public bool HasValue => Time.HasValue;

        public override string ToString() => $"{(Time != null ? Time?.Second.ToString() : "?")}:{(Time != null ? Time?.Millisecond.ToString() : "?")}";

        public void SetTime(DateTimeOffset time)
        {
            if (Time.HasValue)
            {
                _previous = Clone();
            }

            Time = time;
        }

        public MotionStamp Clone() => (MotionStamp)MemberwiseClone();
    }
}