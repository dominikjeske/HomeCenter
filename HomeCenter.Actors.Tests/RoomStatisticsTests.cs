using FluentAssertions;
using HomeCenter.Services.MotionService;
using HomeCenter.Services.MotionService.Model;
using System;
using Xunit;

namespace HomeCenter.Actors.Tests
{
    public class RoomStatisticsTests
    {
        [Fact]
        public void VisitTypeTest()
        {
            var room = CreateRoomStatistic(DefaultConfig());
            var now = new DateTimeOffset(0, TimeSpan.Zero);

            room.MarkMotion(now);
            room.Timeout.Should().Be(TimeSpan.FromSeconds(10));

            room.MarkMotion(now.AddTicks((MotionDefaults.MotionTypePassThru - TimeSpan.FromSeconds(1)).Ticks));
            room.Timeout.Should().Be(TimeSpan.FromSeconds(10));

            room.MarkMotion(now.AddTicks((MotionDefaults.MotionTypePassThru + TimeSpan.FromSeconds(1)).Ticks));
            room.Timeout.Should().Be(TimeSpan.FromSeconds(20));

            room.MarkMotion(now.AddTicks((MotionDefaults.MotionTypeShortVisit - TimeSpan.FromSeconds(1)).Ticks));
            room.Timeout.Should().Be(TimeSpan.FromSeconds(20));

            room.MarkMotion(now.AddTicks((MotionDefaults.MotionTypeShortVisit + TimeSpan.FromSeconds(1)).Ticks));
            room.Timeout.Should().Be(TimeSpan.FromSeconds(30));
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
                    TurnOffTimeout = MotionDefaults.TurnOffTimeOut,
                    MotionTypePassThru = MotionDefaults.MotionTypePassThru,
                    MotionTypeShortVisit = MotionDefaults.MotionTypeShortVisit
                },
                TurnOffTimeout = MotionDefaults.TurnOffTimeOut
            };
        }

        private static RoomStatistic CreateRoomStatistic(AreaDescriptor ad) => new RoomStatistic(new FakeLogger(), ad);
    }
}