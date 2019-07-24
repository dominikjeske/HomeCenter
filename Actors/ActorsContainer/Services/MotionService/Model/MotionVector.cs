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

        public MotionVector(MotionPoint startPoint, MotionPoint endPoint)
        {
            _start = startPoint;
            _end = endPoint;
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

        public override string ToString()
        {
            var time = _end.TimeStamp - _start.TimeStamp;
            return $"{_start} -> {_end} [{time.TotalMilliseconds}ms]";
        }
    }
}