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
        private bool _canCofuse;

        public DateTimeOffset? Time { get; private set; }
        public bool CanConfuze => HasValue && _canCofuse;
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
            _canCofuse = true;
        }

        public void UnConfuze() => _canCofuse = false;

        public MotionStamp Clone() => (MotionStamp)MemberwiseClone();
    }
}