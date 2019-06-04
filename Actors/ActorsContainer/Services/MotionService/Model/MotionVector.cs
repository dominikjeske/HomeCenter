using CSharpFunctionalExtensions;
using System;
using System.Collections.Generic;

namespace HomeCenter.Services.MotionService.Model
{
    /// <summary>
    /// Represent move from one room to other
    /// </summary>
    public class MotionVector : ValueObject, IEquatable<MotionVector>
    {
        private readonly MotionPoint _start;
        private readonly MotionPoint _end;

        public DateTimeOffset StartTime => _start.TimeStamp;
        public DateTimeOffset EndTime => _end.TimeStamp;

        public string StartPoint => _start.Uid;
        public string EndPoint => _end.Uid;

        private readonly List<MotionPoint> _confusionPoints = new List<MotionPoint>();

        public MotionVector()
        {
        }

        public MotionVector(MotionPoint startPoint, MotionPoint endPoint)
        {
            _start = startPoint;
            _end = endPoint;
        }

        public MotionVector WithConfuze(IEnumerable<MotionPoint> confusionPoints)
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

        public bool Contains(MotionPoint p) => _start.Equals(p) || _end.Equals(p);

        public bool Equals(MotionVector other) => base.Equals(other);

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return _start;
            yield return _end;
        }

        public bool EqualsWithEndTime(MotionVector other) => Equals(other) && _end.TimeStamp == other._end.TimeStamp;

        public bool EqualsWithStartTime(MotionVector other) => Equals(other) && _start.TimeStamp == other._start.TimeStamp;

        public bool EqualsBothTimes(MotionVector other) => Equals(other) && _start.TimeStamp == other._start.TimeStamp && _end.TimeStamp == other._end.TimeStamp;

        public IReadOnlyCollection<MotionPoint> ConfusionPoint => _confusionPoints.AsReadOnly();

        public bool IsConfused { get; private set; }

        public override string ToString()
        {
            var time = _end.TimeStamp - _start.TimeStamp;
            var baseFormat = $"{_start} -> {_end} [{time.TotalMilliseconds}ms]";

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