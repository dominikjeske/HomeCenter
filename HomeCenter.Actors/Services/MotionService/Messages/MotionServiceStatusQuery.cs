using HomeCenter.Abstractions;

namespace HomeCenter.Services.MotionService.Commands
{
    public class MotionServiceStatusQuery : Query
    {
        public static MotionServiceStatusQuery Create() => new MotionServiceStatusQuery();
    }
}