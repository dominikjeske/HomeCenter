using CSharpFunctionalExtensions;
using HomeCenter.Utils.Extensions;
using System;
using System.Collections.Generic;

namespace HomeCenter.Services.MotionService.Model
{
    /// <summary>
    /// Represent place and time of movement
    /// </summary>
    public class MotionPoint : ValueObject, IEquatable<MotionPoint>
    {
        public static readonly MotionPoint Empty = new MotionPoint(string.Empty, DateTimeOffset.MinValue);

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

        public bool IsMovePhisicallyPosible(MotionPoint previous, TimeSpan motionMinDiff) => TimeStamp.Between(previous.TimeStamp).IsPossible(motionMinDiff);

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Uid;
            yield return TimeStamp;
        }
    }
}