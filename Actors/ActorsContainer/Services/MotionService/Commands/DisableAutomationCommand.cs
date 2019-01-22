using HomeCenter.Model.Messages.Commands;

namespace HomeCenter.Services.MotionService.Commands
{
    public class DisableAutomationCommand : Command
    {
        public static DisableAutomationCommand Create(string room) => new DisableAutomationCommand { RoomId = room };

        public string RoomId
        {
            get => AsString(MotionProperties.RoomId);
            set => SetProperty(MotionProperties.RoomId, value);
        }

    }
}