using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using ConcurrentCollections;

namespace HomeCenter.Services.MotionService.Model
{
    internal class MotionWindow
    {
        public static readonly ReadOnlyCollection<MotionVector> Default = new List<MotionVector>().AsReadOnly();

        private readonly ConcurrentHashSet<MotionVector> _vectors = new ConcurrentHashSet<MotionVector>();
        private readonly ConcurrentHashSet<MotionVector> _vectorsHistory = new ConcurrentHashSet<MotionVector>();
        private readonly RoomDictionary _roomService;

        public MotionWindow(string place, DateTimeOffset time, RoomDictionary roomService)
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
            if (_vectors.Count == 0) return Default;
            var list = _vectors.ToList();
            _vectors.Clear();
            return list.AsReadOnly();
        }

        public override string ToString() => $"{Start} || {string.Join(" | ", _vectors) ?? "<>"}";
    }
}