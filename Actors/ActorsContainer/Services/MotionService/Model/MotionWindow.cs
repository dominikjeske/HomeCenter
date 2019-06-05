using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace HomeCenter.Services.MotionService.Model
{
    // TODO thread safe
    internal class MotionWindow
    {
        private static readonly ReadOnlyCollection<MotionVector> _noVectors = new List<MotionVector>().AsReadOnly();
        private readonly List<MotionVector> _vectors = new List<MotionVector>();
        private readonly List<MotionVector> _vectorsHistory = new List<MotionVector>();
        private readonly RoomService _roomService;

        public MotionWindow(string place, DateTimeOffset time, RoomService roomService)
        {
            Start = new MotionPoint(place, time);
            _roomService = roomService;
        }

        public MotionPoint Start { get; }

        public MotionWindow AccumulateVector(MotionPoint mp)
        {
            if (_roomService.IsProperVector(Start, mp))
            {
                var vector = new MotionVector(Start, mp);
                if (!_vectorsHistory.Contains(vector))
                {
                    _vectorsHistory.Add(vector);
                    _vectors.Add(vector);
                }
            }

            return this;
        }

        public IReadOnlyCollection<MotionVector> ToVectors()
        {
            if (_vectors.Count == 0) return _noVectors;
            var list = _vectors.ToList();
            _vectors.Clear();
            return list.AsReadOnly();
        }

        public override string ToString() => $"{Start} || {string.Join(" | ", _vectors) ?? "<>"}";
    }
}