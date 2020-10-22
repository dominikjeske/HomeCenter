﻿using System;
using System.Collections.Generic;
using System.Text.Json;
using HomeCenter.Services.Configuration.DTO;
using HomeCenter.Services.MotionService;
using HomeCenter.Services.MotionService.Tests;

namespace HomeCenter.Actors.Tests.Builders
{
    internal class LightAutomationServiceBuilder
    {
        private TimeSpan? _confusionResolutionTime;
        private string _workingTime;

        private readonly Dictionary<string, RoomBuilder> _rooms = new Dictionary<string, RoomBuilder>();

        public RoomBuilder this[string room]
        {
            get { return _rooms[room]; }
        }

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

        public LightAutomationServiceBuilder WithRoom(RoomBuilder room)
        {
            _rooms.Add(room.Name, room);
            return this;
        }

        public ServiceDTO Build()
        {
            var serviceDto = new ServiceDTO
            {
                IsEnabled = true,
                Properties = new Dictionary<string, object>()
            };

            foreach (var room in _rooms.Values)
            {
                AddRoom(serviceDto, room);
            }

            if (_confusionResolutionTime.HasValue)
            {
                serviceDto.Properties.Add(MotionProperties.ConfusionResolutionTime, ToJsonElement(_confusionResolutionTime.ToString()));
            }

            return serviceDto;
        }

        private void AddRoom(ServiceDTO serviceDto, RoomBuilder roomBuilder)
        {
            var area = new AttachedPropertyDTO
            {
                AttachedActor = roomBuilder.Name,
                Properties = new Dictionary<string, object>(),
                Service = "MotionService"
            };

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
            serviceDto.ComponentsAttachedProperties.Add(new AttachedPropertyDTO
            {
                AttachedActor = motionSensor,
                AttachedArea = area,
                Service = "MotionService",
                Properties = new Dictionary<string, object>
                {
                    [MotionProperties.Neighbors] = ToJsonElement(string.Join(", ", neighbors)),
                    [MotionProperties.Lamp] = ToJsonElement(motionSensor)
                }
            });
        }

        private static JsonElement ToJsonElement(object motionSensor)
        {
            return JsonSerializer.Deserialize<JsonElement>(JsonSerializer.Serialize(motionSensor));
        }
    }
}