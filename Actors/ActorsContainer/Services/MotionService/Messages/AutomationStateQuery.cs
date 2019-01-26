using HomeCenter.Model.Messages.Queries;

namespace HomeCenter.Services.MotionService.Commands
{
    public class AutomationStateQuery : Query
    {
        public static AutomationStateQuery Create(string room) => new AutomationStateQuery { RoomId = room };

        public string RoomId
        {
            get => AsString(MotionProperties.RoomId);
            set => SetProperty(MotionProperties.RoomId, value);
        }
    }
}