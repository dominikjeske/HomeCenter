using CSharpFunctionalExtensions;
using System;
using System.Collections.Generic;

namespace HomeCenter.Services.MotionService.Model
{
    public class MotionVector : ValueObject, IEquatable<MotionVector>
    {
        public MotionPoint Start { get; }
        public MotionPoint End { get; }
        private readonly List<MotionPoint> _confusionPoints = new List<MotionPoint>();

        public MotionVector()
        {
        }

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

        public bool Equals(MotionVector other) => base.Equals(other);

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Start;
            yield return End;
        }

        public bool EqualsWithEndTime(MotionVector other) => Equals(other) && End.TimeStamp == other.End.TimeStamp;

        public bool EqualsWithStartTime(MotionVector other) => Equals(other) && Start.TimeStamp == other.Start.TimeStamp;

        public bool EqualsBothTimes(MotionVector other) => Equals(other) && Start.TimeStamp == other.Start.TimeStamp && End.TimeStamp == other.End.TimeStamp;

        public IReadOnlyCollection<MotionPoint> ConfusionPoint => _confusionPoints.AsReadOnly();
        public bool IsConfused { get; private set; }

        public override string ToString()
        {
            var time = End.TimeStamp - Start.TimeStamp;
            var baseFormat = $"{Start} -> {End} [{time.TotalMilliseconds}ms]";

            if (_confusionPoints.Count > 0)
            {
                if (IsConfused)
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