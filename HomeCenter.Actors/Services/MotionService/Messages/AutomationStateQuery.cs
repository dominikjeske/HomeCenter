using HomeCenter.Abstractions;

namespace HomeCenter.Services.MotionService.Commands
{
    public class AutomationStateQuery : Query
    {
        public static AutomationStateQuery Create(string room) => new AutomationStateQuery { RoomId = room };

        public string RoomId
        {
            get => this.AsString(MotionProperties.RoomId);
            set => this.SetProperty(MotionProperties.RoomId, value);
        }
    }
}