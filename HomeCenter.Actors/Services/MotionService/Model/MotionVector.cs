using System;
using System.Collections.Generic;
using CSharpFunctionalExtensions;

namespace HomeCenter.Services.MotionService.Model
{
    /// <summary>
    /// Represent move from one room to other.
    /// </summary>
    public class MotionVector : ValueObject, IEquatable<MotionVector>
    {
        public readonly static MotionVector Empty = new(MotionPoint.Empty, MotionPoint.Empty); 

        public DateTimeOffset StartTime => Start.TimeStamp;

        public DateTimeOffset EndTime => End.TimeStamp;

        public MotionPoint Start { get; }

        public MotionPoint End { get; }

        public string StartPoint => Start.Uid;

        public string EndPoint => End.Uid;

        public MotionVector(MotionPoint startPoint, MotionPoint endPoint)
        {
            Start = startPoint;
            End = endPoint;
        }

        public bool Contains(MotionPoint p) => Start.Equals(p) || End.Equals(p);

        public bool ContainsOnBegin(MotionPoint p) => Start.Equals(p);

        public bool ContainsOnEnd(MotionPoint p) => End.Equals(p);

        public bool Equals(MotionVector other) => base.Equals(other);

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Start;
            yield return End;
        }

        public bool EqualsWithEndTime(MotionVector other) => Equals(other) && End.TimeStamp == other.End.TimeStamp;

        public bool EqualsWithStartTime(MotionVector other) => Equals(other) && Start.TimeStamp == other.Start.TimeStamp;

        public bool EqualsBothTimes(MotionVector other) => Equals(other) && Start.TimeStamp == other.Start.TimeStamp && End.TimeStamp == other.End.TimeStamp;

        public override string ToString()
        {
            var time = End.TimeStamp - Start.TimeStamp;
            return $"{Start} -> {End} [{time.TotalMilliseconds}ms]";
        }
    }
}