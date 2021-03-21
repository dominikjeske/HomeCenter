using FluentAssertions;
using HomeCenter.Services.MotionService;
using HomeCenter.Services.MotionService.Model;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using Xunit;

namespace HomeCenter.Actors.Tests
{
    public class RoomStatisticsTests
    {
        [Fact(DisplayName = "Visit type should change when visit is longer")]
        public void Fact1()
        {
            var room = CreateRoomStatistic(DefaultConfig());
            var now = new DateTimeOffset(0, TimeSpan.Zero);

            room.MarkMotion(now);
            room.VisitType.Should().Be(VisitType.PassThru);

            room.MarkMotion(now.AddTicks((MotionDefaults.MotionTypePassThru - TimeSpan.FromSeconds(1)).Ticks));
            room.VisitType.Should().Be(VisitType.PassThru);

            room.MarkMotion(now.AddTicks((MotionDefaults.MotionTypePassThru + TimeSpan.FromSeconds(1)).Ticks));
            room.VisitType.Should().Be(VisitType.ShortVisit);

            room.MarkMotion(now.AddTicks((MotionDefaults.MotionTypeShortVisit - TimeSpan.FromSeconds(1)).Ticks));
            room.VisitType.Should().Be(VisitType.ShortVisit);

            room.MarkMotion(now.AddTicks((MotionDefaults.MotionTypeShortVisit + TimeSpan.FromSeconds(1)).Ticks));
            room.VisitType.Should().Be(VisitType.LongerVisit);
        }

        [Fact(DisplayName = "First time enter time should be set and reset")]
        public void Fact2()
        {
            var room = CreateRoomStatistic(DefaultConfig());
            var now = new DateTimeOffset(0, TimeSpan.Zero);

            room.FirstEnterTime.Should().BeNull();

            room.MarkMotion(now);
            room.MarkMotion(now.Add(TimeSpan.FromSeconds(1)));
            room.FirstEnterTime.Should().Be(now);

            room.Reset();
            room.FirstEnterTime.Should().BeNull();
        }

        [Fact(DisplayName = "Last motion should be set after move")]
        public void Fact3()
        {
            var room = CreateRoomStatistic(DefaultConfig());
            var now = new DateTimeOffset(0, TimeSpan.Zero);
            var next = now.Add(TimeSpan.FromSeconds(1));

            room.LastMotion.Value.Should().BeNull();

            room.MarkMotion(now);
            room.MarkMotion(next);
            room.LastMotion.Value.Should().Be(next);
            room.LastMotion.Previous.Value.Should().Be(now);
        }

        [Fact(DisplayName = "Number of persons should be set to one after first move")]
        public void Fact4()
        {
            var room = CreateRoomStatistic(DefaultConfig());
            var now = new DateTimeOffset(0, TimeSpan.Zero);

            room.NumberOfPersons.Should().Be(0);

            room.MarkMotion(now);

            room.NumberOfPersons.Should().Be(1);
        }

        [Fact(DisplayName = "Probability should decrease after recalculates")]
        public void Fact5()
        {
            var room = CreateRoomStatistic(DefaultConfig());
            var now = new DateTimeOffset(0, TimeSpan.Zero);

            room.MarkMotion(now);
            room.RecalculateProbability(now.AddSeconds(1));
            room.Probability.Value.Should().BeApproximately(0.9, 0.001);
            room.RecalculateProbability(now.AddSeconds(2));
            room.Probability.Value.Should().BeApproximately(0.8, 0.001);
            room.RecalculateProbability(now.AddSeconds(3));
            room.Probability.Value.Should().BeApproximately(0.7, 0.001);
            room.RecalculateProbability(now.AddSeconds(4));
            room.Probability.Value.Should().BeApproximately(0.6, 0.001);
            room.RecalculateProbability(now.AddSeconds(5));
            room.Probability.Value.Should().BeApproximately(0.5, 0.001);
            room.RecalculateProbability(now.AddSeconds(6));
            room.Probability.Value.Should().BeApproximately(0.4, 0.001);
            room.RecalculateProbability(now.AddSeconds(7));
            room.Probability.Value.Should().BeApproximately(0.3, 0.001);
            room.RecalculateProbability(now.AddSeconds(8));
            room.Probability.Value.Should().BeApproximately(0.2, 0.001);
            room.RecalculateProbability(now.AddSeconds(9));
            room.Probability.Value.Should().BeApproximately(0.1, 0.001);
            room.RecalculateProbability(now.AddSeconds(10));
            room.Probability.Value.Should().BeApproximately(0.0, 0.001);
        }

        [Fact(DisplayName = "Base time out should be recalculated when we have move just after auto turn-off")]
        public void Fact6()
        {
            var area = DefaultConfig();
            var room = CreateRoomStatistic(area);
            var now = new DateTimeOffset(0, TimeSpan.Zero);

            room.BaseTimeOut.Should().Be(area.TurnOffTimeout);

            room.MarkMotion(now);
            room.RecalculateProbability(now.AddSeconds(1));
            room.RecalculateProbability(now.AddSeconds(2));
            room.RecalculateProbability(now.AddSeconds(3));
            room.RecalculateProbability(now.AddSeconds(4));
            room.RecalculateProbability(now.AddSeconds(5));
            room.RecalculateProbability(now.AddSeconds(6));
            room.RecalculateProbability(now.AddSeconds(7));
            room.RecalculateProbability(now.AddSeconds(8));
            room.RecalculateProbability(now.AddSeconds(9));
            room.RecalculateProbability(now.AddSeconds(10));

            room.SetAutoTurnOffTime(now.AddSeconds(10));
            // We have a move just after auto turn off - time out should be tuned for this area
            room.MarkMotion(now.AddSeconds(11));

            room.BaseTimeOut.Should().Be(TimeSpan.FromSeconds(15));
        }

        [Fact(DisplayName = "Base time out should be recalculated when we have move just after auto turn-off")]
        public void Fact7()
        {
            var room = CreateRoomStatistic(DefaultConfig());
            var now = new DateTimeOffset(0, TimeSpan.Zero);

            room.MarkMotion(now);
            room.RecalculateProbability(now); // We have same time

            room.Probability.Value.Should().Be(1, "Probability should not change when recalculate in same time then motion");
        }

        [Fact(DisplayName = "Mark enter in same time then motion")]
        public void Fact8()
        {
            var room = CreateRoomStatistic(DefaultConfig());
            var now = new DateTimeOffset(0, TimeSpan.Zero);

            room.NumberOfPersons.Should().Be(0);

            room.MarkMotion(now);
            room.MarkEnter(now);

            room.NumberOfPersons.Should().Be(1, "Mark enter in same time then motion should be ignored");
        }

        [Fact(DisplayName = "Mark enter should increase number of persons")]
        public void Fact9()
        {
            var room = CreateRoomStatistic(DefaultConfig());
            var now = new DateTimeOffset(0, TimeSpan.Zero);

            room.NumberOfPersons.Should().Be(0);

            room.MarkMotion(now);
            room.MarkEnter(now.AddSeconds(1));

            room.NumberOfPersons.Should().Be(2);
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

        private static RoomStatistic CreateRoomStatistic(AreaDescriptor ad) => new(NullLogger.Instance, ad);
    }
}