using HomeCenter.Model.Messages.Queries;

namespace HomeCenter.Services.MotionService.Commands
{
    public class NumberOfPeopleQuery : Query
    {
        public static NumberOfPeopleQuery Create(string room) => new NumberOfPeopleQuery { RoomId = room };

        public string RoomId
        {
            get => AsString(MotionProperties.RoomId);
            set => SetProperty(MotionProperties.RoomId, value);
        }
    }
}