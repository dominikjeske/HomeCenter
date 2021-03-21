using System;
using System.Xml.Linq;

namespace HomeCenter.Services.MotionService.Model
{
    /// <summary>
    /// Represent motion time in room
    /// </summary>
    public class MotionStamp
    {
        public readonly static MotionStamp Empty = new();

        private MotionStamp _previous = Empty;

        public DateTimeOffset? Value { get; private set; }
        public MotionStamp Previous => _previous ?? Empty;
        public bool HasValue => Value.HasValue;

        public void SetTime(DateTimeOffset time)
        {
            if (Value.HasValue)
            {
                _previous = Clone();
            }

            Value = time;
        }

        public MotionStamp Clone() => (MotionStamp)MemberwiseClone();

        public override string ToString() => $"{Value:HH:mm:ss:ffff}[{Previous.Value:HH:mm:ss:ffff}]";
    }
}