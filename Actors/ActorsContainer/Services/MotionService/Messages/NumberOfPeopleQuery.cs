using HomeCenter.Model.Messages.Queries;

namespace HomeCenter.Services.MotionService.Commands
{
    public class NumberOfPeopleQuery : Query
    {
        public static NumberOfPeopleQuery Create(string room) => new NumberOfPeopleQuery { RoomId = room };

        public string RoomId
        {
            get => this.AsString(MotionProperties.RoomId);
            set => this.SetProperty(MotionProperties.RoomId, value);
        }
    }
}