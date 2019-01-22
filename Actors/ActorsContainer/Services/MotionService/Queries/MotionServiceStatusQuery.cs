using HomeCenter.Model.Messages.Queries;

namespace HomeCenter.Services.MotionService.Commands
{
    public class MotionServiceStatusQuery : Query
    {
        public static MotionServiceStatusQuery Create() => new MotionServiceStatusQuery();
    }
}