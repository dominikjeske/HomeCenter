using HomeCenter.Model.Messages.Queries;

namespace HomeCenter.Services.MotionService.Commands
{

    public class AreaDescriptorQuery : Query
    {
        public static AreaDescriptorQuery Create(string room) => new AreaDescriptorQuery { RoomId = room };

        public string RoomId
        {
            get => AsString(MotionProperties.RoomId);
            set => SetProperty(MotionProperties.RoomId, value);
        }
    }
}