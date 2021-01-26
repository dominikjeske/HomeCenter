using HomeCenter.Actors.Tests.Fakes;
using HomeCenter.Services.MotionService;
using HomeCenter.Services.MotionService.Model;
using System;

namespace HomeCenter.Actors.Tests
{
    public class RoomStatisticsTests
    {
        public void Test()
        {
            var roomStatistic = CreateRoomStatistic
            (
                CreateMotionConfigFromDefauls() with
                {
                    TurnOffTimeout = TimeSpan.FromSeconds(20)
                }
            );


        }

        private static MotionConfiguration CreateMotionConfigFromDefauls()
        {
            return new MotionConfiguration
            {
                ConfusionResolutionTime = MotionDefaults.ConfusionResolutionTime,
                ConfusionResolutionTimeOut = MotionDefaults.ConfusionResolutionTimeOut,
                DecreaseLeavingFactor = MotionDefaults.DecreaseLeavingFactor,
                MotionMinDiff = MotionDefaults.MotionMinDiff,
                MotionTimeWindow = MotionDefaults.MotionTimeWindow,
                PeriodicCheckTime = MotionDefaults.PeriodicCheckTime,
                TurnOffTimeoutExtenderFactor = MotionDefaults.TurnOffTimeoutExtenderFactor,
                TurnOffTimeout = MotionDefaults.TurnOffTimeOut

            };
        }

        private static RoomStatistic CreateRoomStatistic(MotionConfiguration mc)
        {
            return new RoomStatistic(new FakeLogger<RoomStatisticsTests>(), "TestRoom", mc);
        }
    }
}