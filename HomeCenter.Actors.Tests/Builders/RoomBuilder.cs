using System;
using System.Collections.Generic;

namespace HomeCenter.Services.MotionService.Tests
{
    internal class RoomBuilder
    {
        public Dictionary<string, object> Properties { get; private set; } = new Dictionary<string, object>();
        public Dictionary<string, DetectorDescription> Detectors { get; private set; } = new Dictionary<string, DetectorDescription>();

        public string Name { get; private set; }

        public RoomBuilder(string name)
        {
            Name = name;
        }

        public RoomBuilder WithDetector(string detectorName, List<string> neighbors)
        {
            Detectors.Add(detectorName, new DetectorDescription
            {
                DetectorName = detectorName,
                Neighbors = neighbors
            });
            return this;
        }

        public RoomBuilder WithTimeout(TimeSpan timeSpan)
        {
            WithProperty(MotionProperties.TurnOffTimeout, timeSpan.ToString());
            return this;
        }

        public RoomBuilder WithProperty(string propertyKey, object propertyValue)
        {
            Properties.Add(propertyKey, propertyValue);

            return this;
        }
    }
}