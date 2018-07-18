using CSharpFunctionalExtensions;
using System;
using System.Collections.Generic;

namespace Wirehome.Motion.Model
{
    public class MotionVector : ValueObject<MotionVector>, IEquatable<MotionVector>
    {
        public MotionPoint Start { get;}
        public MotionPoint End { get; }
        private readonly List<MotionPoint> _confusionPoints = new List<MotionPoint>();

        public MotionVector() { }
        public MotionVector(MotionPoint startPoint, MotionPoint endPoint)
        {
            Start = startPoint;
            End = endPoint;
        }

        public MotionVector Confuze(IEnumerable<MotionPoint> confusionPoints)
        {
            _confusionPoints.AddRange(confusionPoints);
            IsConfused = true;
            return this;
        }

        public MotionVector UnConfuze()
        {
            IsConfused = false;
            return this;
        }

        public bool Contains(MotionPoint p) => Start.Equals(p) || End.Equals(p);
        protected override bool EqualsCore(MotionVector other) => other.Start.Equals(Start) && other.End.Equals(End);
        public bool Equals(MotionVector other) => base.Equals(other);

        protected override int GetHashCodeCore()
        {
            unchecked
            {
                return ((Start?.GetHashCode() ?? 0) * 397) ^ End.GetHashCode();
            }
        }

        public bool EqualsWithEndTime(MotionVector other) => EqualsCore(other) && End.TimeStamp == other.End.TimeStamp;
        public bool EqualsWithStartTime(MotionVector other) => EqualsCore(other) && Start.TimeStamp == other.Start.TimeStamp;
        public bool EqualsBothTimes(MotionVector other) => EqualsCore(other) && Start.TimeStamp == other.Start.TimeStamp && End.TimeStamp == other.End.TimeStamp;

        public IReadOnlyCollection<MotionPoint> ConfusionPoint => _confusionPoints.AsReadOnly();
        public bool IsConfused { get; private set; }

        public override string ToString()
        {
            var time = End.TimeStamp - Start.TimeStamp;
            var baseFormat = $"{Start} -> {End} [{time.TotalMilliseconds}ms]";

            if (_confusionPoints.Count > 0)
            {
                if(IsConfused)
                {
                    return baseFormat + $" | Confusion: {string.Join(", ", ConfusionPoint)}";
                }
                else
                {
                    return baseFormat + " | Unconfused";
                }
            }
            else
            {
                return baseFormat;
            }
        }
    }
}
