using HomeCenter.Actors.Tests.Fakes;
using HomeCenter.Services.MotionService;
using HomeCenter.Services.MotionService.Model;

namespace HomeCenter.Actors.Tests
{
    public class RoomStatisticsTests
    {
        public void Test()
        {
            //var roomStatistic = CreateRoomStatistic(DefaultConfig() with {TurnOffTimeout = TimeSpan.FromSeconds(20)});
        }

        private static AreaDescriptor DefaultConfig()
        {
            return new AreaDescriptor
            {
                Motion = new MotionConfiguration
                {
                    ConfusionResolutionTime = MotionDefaults.ConfusionResolutionTime,
                    ConfusionResolutionTimeOut = MotionDefaults.ConfusionResolutionTimeOut,
                    DecreaseLeavingFactor = MotionDefaults.DecreaseLeavingFactor,
                    MotionMinDiff = MotionDefaults.MotionMinDiff,
                    MotionTimeWindow = MotionDefaults.MotionTimeWindow,
                    PeriodicCheckTime = MotionDefaults.PeriodicCheckTime,
                    TurnOffTimeoutExtenderFactor = MotionDefaults.TurnOffTimeoutExtenderFactor,
                    TurnOffTimeout = MotionDefaults.TurnOffTimeOut
                }
            };
        }

        //private static RoomStatistic CreateRoomStatistic(AreaDescriptor ad)
        //{
        //    return new RoomStatistic(new FakeLogger<RoomStatisticsTests>(), "TestRoom", ad);
        //}
    }
}