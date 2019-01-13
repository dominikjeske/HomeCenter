using HomeCenter.Model.Core;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;

namespace HomeCenter.Services.MotionService.Model
{
    public class RoomInitializer
    {
        public RoomInitializer(string motionDetectorUid, IEnumerable<string> neighbors, string lamp, IEnumerable<IEventDecoder> eventDecoders, AreaDescriptor areaInitializer = null)
        {
            MotionDetectorUid = motionDetectorUid;
            Neighbors = neighbors;
            Lamp = lamp;
            AreaInitializer = areaInitializer;
            EventDecoders = eventDecoders;
        }

        public string MotionDetectorUid { get; }
        public IEnumerable<string> Neighbors { get; }
        public string Lamp { get; }
        public AreaDescriptor AreaInitializer { get; }
        public IEnumerable<IEventDecoder> EventDecoders { get; }

        public Room ToRoom(MotionConfiguration config, IConcurrencyProvider concurrencyProvider, ILogger logger, IMessageBroker messageBroker)
        {
            return new Room(MotionDetectorUid, Neighbors, Lamp, concurrencyProvider, logger, messageBroker, AreaInitializer, config, EventDecoders);
        }
    }
}