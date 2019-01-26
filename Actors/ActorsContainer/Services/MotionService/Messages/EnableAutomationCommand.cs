using HomeCenter.Model.Messages.Commands;

namespace HomeCenter.Services.MotionService.Commands
{
    public class EnableAutomationCommand : Command
    {
        public static EnableAutomationCommand Create(string room) => new EnableAutomationCommand { RoomId = room };

        public string RoomId
        {
            get => AsString(MotionProperties.RoomId);
            set => SetProperty(MotionProperties.RoomId, value);
        }
    }
}