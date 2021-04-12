using HomeCenter.Abstractions;
using HomeCenter.Services.MotionService.Model;
using System.Collections.Generic;

namespace HomeCenter.Services.MotionService.Commands
{
    public class RoomStateQuery : Query
    {
        public static RoomStateQuery Create(string room) => new RoomStateQuery { RoomId = room };

        public string RoomId
        {
            get => this.AsString(MotionProperties.RoomId);
            set => this.SetProperty(MotionProperties.RoomId, value);
        }
    }

    public class RoomState
    {
        public int NumberOfPersosn { get; set; }
        public bool AutomationEnabled { get; set; }
        public bool HasConfusions { get; set; }
    }
}