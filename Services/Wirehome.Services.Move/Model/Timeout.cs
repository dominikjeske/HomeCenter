using System;

namespace Wirehome.Motion.Model
{
    public class Timeout
    {
        private TimeSpan _baseTime;
        private TimeSpan _currentExtension = TimeSpan.FromTicks(0);
        private int _counter;
        private float _incrementFactor; 

        public TimeSpan Value => _baseTime + _currentExtension;

        public Timeout(TimeSpan baseTime, float incrementFactor)
        {
            _baseTime = baseTime;
            _incrementFactor = incrementFactor;
        }

        public void UpdateBaseTime(TimeSpan baseTime) => _baseTime = baseTime;
        public void IncrementCounter()
        {
            _counter++;
            var factor = _counter * _incrementFactor;
            var value = TimeSpan.FromTicks((long)(Value.Ticks * factor));
            _currentExtension = _currentExtension.Add(value);
        }


        public void Reset()
        {
            _currentExtension = TimeSpan.FromTicks(0);
            _counter = 0;
        }
    }
}