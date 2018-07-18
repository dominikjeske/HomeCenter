using CSharpFunctionalExtensions;
using System;
using Wirehome.Model.Extensions;

namespace Wirehome.Motion.Model
{
    //TODO move to new version
    public class MotionPoint : ValueObject<MotionPoint>, IEquatable<MotionPoint>
    {
        public string Uid { get; }
        public DateTimeOffset TimeStamp { get; }

        public MotionPoint(string place, DateTimeOffset time)
        {
            Uid = place;
            TimeStamp = time;
        }

        public MotionVector ToVector(MotionPoint start) => new MotionVector(start, this);
        protected override bool EqualsCore(MotionPoint other) => Equals(Uid, other.Uid);
        public bool Equals(MotionPoint other) => base.Equals(other);
        public override string ToString() => $"{Uid}: {TimeStamp:ss:fff}";

        protected override int GetHashCodeCore()
        {
            unchecked
            {
                return ((Uid?.GetHashCode() ?? 0) * 397) ^ TimeStamp.GetHashCode();
            }
        }

        public bool IsMovePhisicallyPosible(MotionPoint point, TimeSpan motionMinDiff) => TimeStamp.IsMovePhisicallyPosible(point.TimeStamp, motionMinDiff);
    }
}
