using HomeCenter.Actors.Tests.Helpers;
using HomeCenter.Services.Configuration.DTO;
using HomeCenter.Services.MotionService;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text.Json;

namespace HomeCenter.Actors.Tests.Builders
{
    internal class LightServiceBuilder
    {
        private TimeSpan? _confusionResolutionTime;
        private string _workingTime = string.Empty;

        private readonly Dictionary<string, RoomBuilder> _rooms = new Dictionary<string, RoomBuilder>();

        public RoomBuilder this[string room]
        {
            get { return _rooms[room]; }
        }

        public LightServiceBuilder WithDefaultRooms()
        {
            WithRoom(new RoomBuilder(Rooms.Hallway).WithDetector(Detectors.hallwayToilet, new List<string> { Detectors.hallwayLivingRoom, Detectors.kitchen, Detectors.staircaseDetector, Detectors.toilet, Detectors.badroomDetector })
                                                           .WithDetector(Detectors.hallwayLivingRoom, new List<string> { Detectors.livingRoom, Detectors.bathroom, Detectors.hallwayToilet }));
            WithRoom(new RoomBuilder(Rooms.Badroom).WithDetector(Detectors.badroomDetector, new List<string> { Detectors.hallwayToilet }));
            WithRoom(new RoomBuilder(Rooms.Balcony).WithDetector(Detectors.balconyDetector, new List<string> { Detectors.livingRoom }));
            WithRoom(new RoomBuilder(Rooms.Bathroom).WithDetector(Detectors.bathroom, new List<string> { Detectors.hallwayLivingRoom }));
            WithRoom(new RoomBuilder(Rooms.Kitchen).WithDetector(Detectors.kitchen, new List<string> { Detectors.hallwayToilet }));
            WithRoom(new RoomBuilder(Rooms.Livingroom).WithDetector(Detectors.livingRoom, new List<string> { Detectors.balconyDetector, Detectors.hallwayLivingRoom }));
            WithRoom(new RoomBuilder(Rooms.Staircase).WithDetector(Detectors.staircaseDetector, new List<string> { Detectors.hallwayToilet }));
            WithRoom(new RoomBuilder(Rooms.Toilet).WithDetector(Detectors.toilet, new List<string> { Detectors.hallwayToilet }).WithProperty(MotionProperties.MaxPersonCapacity, 1));

            return this;
        }

        public LightServiceBuilder WithConfusionResolutionTime(TimeSpan confusionResolutionTime)
        {
            _confusionResolutionTime = confusionResolutionTime;
            return this;
        }

        public LightServiceBuilder WithWorkingTime(string wortkingTime)
        {
            _workingTime = wortkingTime;
            return this;
        }

        public LightServiceBuilder WithRoomConfig(string roomId, Action<RoomBuilder> roomAction)
        {
            roomAction(_rooms[roomId]);
            return this;
        }

        public LightServiceBuilder WithRoom(RoomBuilder room)
        {
            _rooms.Add(room.Name, room);
            return this;
        }

        public ServiceDTO Build()
        {
            var serviceDto = new ServiceDTO("LightAutomation", "Service", ImmutableDictionary<string, object>.Empty, ImmutableDictionary<string, string>.Empty, true, Enumerable.Empty<AttachedPropertyDTO>(), Enumerable.Empty<AttachedPropertyDTO>());

            foreach (var room in _rooms.Values)
            {
                AddRoom(serviceDto, room);
            }

            if (_confusionResolutionTime.HasValue)
            {
                var confuseValue = _confusionResolutionTime.ToString();
                if (confuseValue is not null)
                {
                    serviceDto.Properties.Add(MotionProperties.ConfusionResolutionTime, ToJsonElement(confuseValue));
                }
            }

            return serviceDto;
        }

        private void AddRoom(ServiceDTO serviceDto, RoomBuilder roomBuilder)
        {
            var area = new AttachedPropertyDTO("Propery", string.Empty, ImmutableDictionary<string, object>.Empty, "MotionService", roomBuilder.Name, "");

            if (!string.IsNullOrWhiteSpace(_workingTime))
            {
                area.Properties[MotionProperties.WorkingTime] = ToJsonElement(_workingTime);
            }

            foreach (var property in roomBuilder.Properties)
            {
                area.Properties[property.Key] = ToJsonElement(property.Value);
            }

            foreach (var detector in roomBuilder.Detectors.Values)
            {
                AddMotionSensor(detector.DetectorName, roomBuilder.Name, detector.Neighbors, serviceDto);
            }

            serviceDto.AreasAttachedProperties.Add(area);
        }

        private void AddMotionSensor(string motionSensor, string area, IEnumerable<string> neighbors, ServiceDTO serviceDto)
        {
            serviceDto.ComponentsAttachedProperties.Add(new AttachedPropertyDTO("AttachedProp", string.Empty, new Dictionary<string, object>
            {
                [MotionProperties.Neighbors] = ToJsonElement(string.Join(", ", neighbors)),
                [MotionProperties.Lamp] = ToJsonElement(motionSensor)
            }, "MotionService", motionSensor, area));
        }

        private static JsonElement ToJsonElement(object motionSensor)
        {
            return JsonSerializer.Deserialize<JsonElement>(JsonSerializer.Serialize(motionSensor));
        }
    }
}