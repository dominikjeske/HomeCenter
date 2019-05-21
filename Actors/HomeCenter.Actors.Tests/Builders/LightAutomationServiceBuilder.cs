using HomeCenter.Services.Configuration.DTO;
using System;
using System.Collections.Generic;

namespace HomeCenter.Services.MotionService.Tests
{
    internal class LightAutomationServiceBuilder
    {
        private TimeSpan? _confusionResolutionTime;
        private string _workingTime;

        private readonly Dictionary<string, AreaDescription> _areas = new Dictionary<string, AreaDescription>();
        private readonly Dictionary<string, DetectorDescription> _detectors = new Dictionary<string, DetectorDescription>();

        public LightAutomationServiceBuilder WithConfusionResolutionTime(TimeSpan confusionResolutionTime)
        {
            _confusionResolutionTime = confusionResolutionTime;
            return this;
        }

        public LightAutomationServiceBuilder WithWorkingTime(string wortkingTime)
        {
            _workingTime = wortkingTime;
            return this;
        }

        public LightAutomationServiceBuilder WithArea(string areaName)
        {
            _areas.Add(areaName, new AreaDescription
            {
                AreaName = areaName,
            });
            return this;
        }

        public LightAutomationServiceBuilder WithAreaProperty(string areaName, string propertyKey, string propertyValue)
        {
            _areas[areaName].Properties.Add(propertyKey, propertyValue);

            return this;
        }

        public LightAutomationServiceBuilder WithDetector(string detectorName, string areaName, List<string> neighbors)
        {
            _detectors.Add(detectorName, new DetectorDescription
            {
                DetectorName = detectorName,
                AreaName = areaName,
                Neighbors = neighbors
            });
            return this;
        }

        private class AreaDescription
        {
            public string AreaName { get; set; }

            public Dictionary<string, string> Properties = new Dictionary<string, string>();
        }

        private class DetectorDescription
        {
            public string DetectorName { get; set; }
            public string AreaName { get; set; }
            public List<string> Neighbors { get; set; } = new List<string>();

            public Dictionary<string, string> Properties = new Dictionary<string, string>();
        }

        public ServiceDTO Build()
        {
            var serviceDto = new ServiceDTO
            {
                IsEnabled = true,
                Properties = new Dictionary<string, string>()
            };

            foreach (var area in _areas.Values)
            {
                AddArea(serviceDto, area.AreaName, area.Properties);
            }

            foreach (var detector in _detectors.Values)
            {
                AddMotionSensor(detector.DetectorName, detector.AreaName, detector.Neighbors, serviceDto);
            }

            if (_confusionResolutionTime.HasValue)
            {
                serviceDto.Properties.Add(MotionProperties.ConfusionResolutionTime, _confusionResolutionTime.ToString());
            }

            return serviceDto;
        }

        private void AddArea(ServiceDTO serviceDto, string areaName, IDictionary<string, string> properties = null)
        {
            var area = new AttachedPropertyDTO
            {
                AttachedActor = areaName,
                Properties = new Dictionary<string, string>()
            };

            if (!string.IsNullOrWhiteSpace(_workingTime))
            {
                area.Properties[MotionProperties.WorkingTime] = _workingTime;
            }

            if (properties != null)
            {
                foreach (var property in properties)
                {
                    area.Properties[property.Key] = property.Value;
                }
            }

            serviceDto.AreasAttachedProperties.Add(area);
        }

        private void AddMotionSensor(string motionSensor, string area, IEnumerable<string> neighbors, ServiceDTO serviceDto)
        {
            serviceDto.ComponentsAttachedProperties.Add(new AttachedPropertyDTO
            {
                AttachedActor = motionSensor,
                AttachedArea = area,
                Properties = new Dictionary<string, string>
                {
                    [MotionProperties.Neighbors] = string.Join(", ", neighbors),
                    [MotionProperties.Lamp] = motionSensor
                }
            });
        }
    }
}