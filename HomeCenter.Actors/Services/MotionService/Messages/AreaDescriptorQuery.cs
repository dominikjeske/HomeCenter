namespace HomeCenter.Services.MotionService.Commands
{
    public class AreaDescriptorQuery : Query
    {
        public static AreaDescriptorQuery Create(string room) => new AreaDescriptorQuery { RoomId = room };

        public string RoomId
        {
            get => this.AsString(MotionProperties.RoomId);
            set => this.SetProperty(MotionProperties.RoomId, value);
        }
    }
}