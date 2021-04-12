using HomeCenter.Abstractions;

namespace HomeCenter.Services.MotionService.Commands
{
    public class MotionServiceStateQuery : Query
    {
        public static MotionServiceStateQuery Create() => new MotionServiceStateQuery();
    }
}