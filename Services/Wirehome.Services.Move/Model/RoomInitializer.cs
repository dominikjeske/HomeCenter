using System.Reactive.Concurrency;
using System.Collections.Generic;
using Wirehome.ComponentModel.Components;
using Wirehome.Core.Services.Logging;
using Wirehome.Contracts.Environment;

namespace Wirehome.Motion.Model
{
    public class RoomInitializer
    {
        public RoomInitializer(string motionDetectorUid, IEnumerable<string> neighbors, Component lamp, IEnumerable<IEventDecoder> eventDecoders, AreaDescriptor areaInitializer = null)
        {
            MotionDetectorUid = motionDetectorUid;
            Neighbors = neighbors;
            Lamp = lamp;
            AreaInitializer = areaInitializer;
            EventDecoders = eventDecoders;
        }

        public string MotionDetectorUid { get; }
        public IEnumerable<string> Neighbors { get; }
        public Component Lamp { get; }
        public AreaDescriptor AreaInitializer { get; }
        public IEnumerable<IEventDecoder> EventDecoders { get; }

        public Room ToRoom(MotionConfiguration config, IDaylightService daylightService, IConcurrencyProvider concurrencyProvider, ILogger logger)
        {
            return new Room(MotionDetectorUid, Neighbors, Lamp, daylightService, concurrencyProvider, logger, AreaInitializer, config, EventDecoders);
        }
    }
}


