using CSharpFunctionalExtensions;
using HomeCenter.Utils.Extensions;
using System;
using System.Collections.Generic;

namespace HomeCenter.Services.MotionService.Model
{
    public class MotionPoint : ValueObject, IEquatable<MotionPoint>
    {
        public string Uid { get; }
        public DateTimeOffset TimeStamp { get; }

        public MotionPoint(string place, DateTimeOffset time)
        {
            Uid = place;
            TimeStamp = time;
        }

        public MotionVector ToVector(MotionPoint start) => new MotionVector(start, this);

        public bool Equals(MotionPoint other) => base.Equals(other);

        public override string ToString() => $"{Uid}: {TimeStamp:ss:fff}";

        public bool IsMovePhisicallyPosible(MotionPoint point, TimeSpan motionMinDiff) => TimeStamp.IsMovePhisicallyPosible(point.TimeStamp, motionMinDiff);

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Uid;
        }
    }
}