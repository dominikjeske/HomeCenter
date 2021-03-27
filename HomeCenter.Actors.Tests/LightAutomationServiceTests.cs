using FluentAssertions;
using HomeCenter.Abstractions;
using HomeCenter.Actors.Tests.Builders;
using HomeCenter.Actors.Tests.Helpers;
using HomeCenter.Services.Configuration.DTO;
using HomeCenter.Services.MotionService.Commands;
using HomeCenter.Services.MotionService.Model;
using Microsoft.Reactive.Testing;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace HomeCenter.Services.MotionService.Tests
{
    // Count number of people in house

    /* RavenDB query
         from @all_docs as e
          where e.lvl = "Information"
            and e.SourceContext = "HomeCenter.Services.MotionService.Room"
        select {
             Source : e.Room,
             Message: e.mt,
             Level: e.lvl,
             Time: e.RxTime,
             Event: e.EventId.Name,
             Vector: e.vector,
             Status: e.VectorStatus,
             BaseTimeOut: e.Statistics.BaseTimeOut,
             VisitType: e.Statistics.VisitType,
             Previous: e.Previous,
             Probability: e.Statistics.Probability,
             FirstEnterTime: e.Statistics.FirstEnterTime,
             NumberOfPersons: e.Statistics.NumberOfPersons,
             Delta: e.delta
         }
    */

    //                                      STAIRCASE [O]<1+>
    //  ________________________________________<_    __________________________
    // |        |                |                       |                      |
    // |        |                  [HL]<1+>  HALLWAY     |<                     |
    // |   B    |                |<            [H]<1+>                          |
    // |   A                     |___   ______           |       BADROOM        |
    // |   L    |                |            |          |         [S]<1+>      |
    // |   C    |                |            |          |                      |
    // |   O    |                |            |          |______________________|
    // |   N    |   LIVINGROOM  >|            |          |<                     |
    // |   Y    |      [L]<1+>   |  BATHROOM  | [HT]<1+> |                      |
    // |        |                |   [B]<1+> >|___v  ____|                      |
    // | [W]<1+>|                |            |          |       KITCHEN        |
    // |        |                |            |  TOILET  |         [K]<1+>      |
    // |        |                |            |  [T]<1>  |                      |
    // |_______v|________________|____________|_____v____|______________________|
    //
    // LEGEND: v/< - Motion Detector
    //         <x> - Max number of persons

    public class LightAutomationServiceTests : ReactiveTest
    {
        private static readonly bool _useRavenDbLogs = true;
        private readonly bool _cleanLogsBeforeRun = true;

        static LightAutomationServiceTests()
        {
            if (_useRavenDbLogs)
            {
                var ravenConfig = new RavenDbConfigurator();
                _useRavenDbLogs = ravenConfig.CheckDbConnection();
                ravenConfig.Dispose();
            }
        }

        [Fact]
        public void WorkingTime_DuringDaylight_ShuldPowerOnDayLights()
        {
            var servieConfig = GetServiceBuilder().WithWorkingTime(WorkingTime.DayLight).Build();
            using var env = GetEnviromentBuilder(servieConfig).WithMotions(new Dictionary<int, string>
            {
                { 500, Detectors.toilet },
                { 1500, Detectors.kitchen },
                { 2000, Detectors.livingRoom }
            }).Build();

            SystemTime.Set(TimeSpan.FromHours(12));

            env.AdvanceToEnd();

            env.LampState(Detectors.toilet).Should().BeTrue();
            env.LampState(Detectors.kitchen).Should().BeTrue();
            env.LampState(Detectors.livingRoom).Should().BeTrue();
        }

        [Fact]
        public void WorkingTime_AfterDusk_ShouldNotPowerOnDayLight()
        {
            var servieConfig = GetServiceBuilder().WithWorkingTime(WorkingTime.DayLight).Build();
            using var env = GetEnviromentBuilder(servieConfig).WithMotions(new Dictionary<int, string>
            {
                { 500, Detectors.toilet },
                { 1500, Detectors.kitchen },
                { 2000, Detectors.livingRoom }
            }).Build();

            SystemTime.Set(TimeSpan.FromHours(21));

            env.AdvanceToEnd();

            env.LampState(Detectors.toilet).Should().BeFalse();
            env.LampState(Detectors.kitchen).Should().BeFalse();
            env.LampState(Detectors.livingRoom).Should().BeFalse();
        }

        [Fact]
        public void WorkingTime_DuringDaylight_ShuldNotPowerOnNightLight()
        {
            var servieConfig = GetServiceBuilder().WithWorkingTime(WorkingTime.AfterDusk).Build();
            using var env = GetEnviromentBuilder(servieConfig).WithMotions(new Dictionary<int, string>
            {
                { 500, Detectors.toilet },
                { 1500, Detectors.kitchen },
                { 2000, Detectors.livingRoom }
            }).Build();

            SystemTime.Set(TimeSpan.FromHours(12));

            env.AdvanceToEnd();

            env.LampState(Detectors.toilet).Should().BeFalse();
            env.LampState(Detectors.kitchen).Should().BeFalse();
            env.LampState(Detectors.livingRoom).Should().BeFalse();
        }

        [Fact]
        public async Task Automation_WhenDisabled_ShouldNoturnOnLights()
        {
            using var env = GetEnviromentBuilder(GetServiceBuilder().Build()).WithMotions(new Dictionary<int, string>
            {
                { 500, Detectors.toilet }
            }).Build();

            env.SendCommand(DisableAutomationCommand.Create(Detectors.toilet));
            env.AdvanceToEnd();

            env.LampState(Detectors.toilet).Should().BeFalse();
            var automationState = await env.Query<bool>(AutomationStateQuery.Create(Detectors.toilet));
            automationState.Should().BeFalse();
        }

        [Fact]
        public void Automation_WhenReEnabled_ShouldTurnOnLights()
        {
            using var env = GetEnviromentBuilder(GetServiceBuilder().Build()).WithMotions(new Dictionary<int, string>
            {
                { 500, Detectors.toilet },
                { 2500, Detectors.toilet }
            }).Build();

            env.SendCommand(DisableAutomationCommand.Create(Detectors.toilet));
            env.SendAfterFirstMove(_ => env.SendCommand(EnableAutomationCommand.Create(Detectors.toilet)));

            env.LampState(Detectors.toilet).Should().BeFalse();
            env.AdvanceToEnd();
            env.LampState(Detectors.toilet).Should().BeTrue();
        }

        // *[Confusion], ^[Resolved]
        //  ___________________________________________   __________________________
        // |        |                |                       |                      |
        // |        |                                                               |
        // |        |                |                       |                      |
        // |                         |___   ______           |                      |
        // |        |                |            |          |                      |
        // |        |                |            |          |                      |
        // |        |                |            |          |______________________|
        // |        |                |            |          |                      |
        // |        |                |            |    1                            |
        // |        |                |            |____  ____|                      |
        // |        |                |            |          |                      |
        // |        |                |            |    0     |                      |
        // |        |                |            |          |                      |
        // |________|________________|____________|__________|______________________|
        [Fact]
        public void Leave_FromOnePeopleRoomWithNoConfusion_ShouldTurnOffLightImmediately()
        {
            using var env = GetEnviromentBuilder(GetServiceBuilder().Build()).WithMotions(new Dictionary<int, string>
            {
                { 500, Detectors.toilet },
                { 1500, Detectors.hallwayToilet }
            }).Build();

            env.AdvanceToEnd();

            env.LampState(Detectors.toilet).Should().BeFalse();
        }

        // *[Confusion], ^[Resolved]
        //  ___________________________________________   __________________________
        // |        |                |                       |                      |
        // |        |                     3                                         |
        // |        |                |                       |                      |
        // |                         |___   ______           |                      |
        // |        |                |            |          |                      |
        // |        |                |            |          |                      |
        // |        |                |            |          |______________________|
        // |        |                |            |          |                      |
        // |        |                |            |    2*       1,4^                |
        // |        |                |            |____  ____|                      |
        // |        |                |            |          |                      |
        // |        |                |            |    0     |                      |
        // |        |                |            |          |                      |
        // |________|________________|____________|__________|______________________|
        [Fact]
        public void Leave_FromOnePeopleRoomWithConfusion_ShouldTurnOffAfterResolvingConfusion()
        {
            var servieConfig = GetServiceBuilder().WithConfusionResolutionTime(MotionDefaults.ConfusionResolutionTime).Build();
            using var env = GetEnviromentBuilder(servieConfig).WithMotions(new Dictionary<int, string>
            {
                { 500, Detectors.toilet },
                { 1000, Detectors.kitchen },
                { 1500, Detectors.hallwayToilet },
                { 2000, Detectors.hallwayLivingRoom },
                { 3000, Detectors.kitchen },
            }).Build();

            env.AdvanceToIndex(2, MotionDefaults.ConfusionResolutionTime, true);

            env.LampState(Detectors.toilet).Should().BeFalse();
            env.LampState(Detectors.kitchen).Should().BeTrue();
        }

        // *[Confusion], ^[Resolved]
        //  ___________________________________________   __________________________
        // |        |                |                       |                      |
        // |        |                   1                      0                    |
        // |        |                |                       |                      |
        // |                         |___   ______           |                      |
        // |        |                |            |          |                      |
        // |        |                |            |          |                      |
        // |        |                |            |          |______________________|
        // |        |                |            |    2*    |                      |
        // |        |                |            |                                 |
        // |        |                |            |____  ____|                      |
        // |        |                |            |          |                      |
        // |        |                |            |          |                      |
        // |        |                |            |          |                      |
        // |________|________________|____________|__________|______________________|
        [Fact]
        public void Leave_FromSeparateRoomsIntoOne_ShouldTurnOffLightInBoth()
        {
            var servieConfig = GetServiceBuilder().WithConfusionResolutionTime(MotionDefaults.ConfusionResolutionTime).Build();
            using var env = GetEnviromentBuilder(servieConfig).WithMotions(new Dictionary<int, string>
            {
                { 500, Detectors.badroomDetector },
                { 1000, Detectors.hallwayLivingRoom },
                { 1500, Detectors.hallwayToilet }
            }).Build();

            env.AdvanceToIndex(2, MotionDefaults.ConfusionResolutionTime, true);

            env.LampState(Detectors.badroomDetector).Should().BeFalse();
            env.LampState(Detectors.hallwayLivingRoom).Should().BeFalse();
            env.LampState(Detectors.hallwayToilet).Should().BeTrue();
        }

        // *[Confusion], ^[Resolved]
        //  ___________________________________________   __________________________
        // |        |                |                       |                      |
        // |        |                   1                                           |
        // |        |                |                       |                      |
        // |                         |___   ______           |                      |
        // |        |                |            |          |                      |
        // |        |                |    4^      |          |                      |
        // |        |                |            |          |______________________|
        // |        |                |            |          |                      |
        // |        |                |            |    3*                           |
        // |        |                |            |____  ____|                      |
        // |        |                |            |          |                      |
        // |        |                |            |    2     |                      |
        // |        |                |            |          |                      |
        // |________|________________|____________|__________|______________________|
        [Fact]
        public void Leave_ToOtherRoom_ShouldSpeedUpResolutionInNeighbor()
        {
            var servieConfig = GetServiceBuilder().WithConfusionResolutionTime(MotionDefaults.ConfusionResolutionTime).Build();
            using var env = GetEnviromentBuilder(servieConfig).WithMotions(new Dictionary<int, string>
            {
                { 500, Detectors.hallwayLivingRoom },
                { 1000, Detectors.toilet },
                { 1500, Detectors.hallwayToilet },
                { 1600, Detectors.bathroom }     // This move give us proper move to bathroom so other confusions can resolve faster because this decrease their probability
            }).Build();

            env.AdvanceToEnd((MotionDefaults.ConfusionResolutionTime / 2)); // We should resolve confusion 2x faster

            env.LampState(Detectors.toilet).Should().BeFalse();
            env.LampState(Detectors.hallwayLivingRoom).Should().BeFalse();
            env.LampState(Detectors.bathroom).Should().BeTrue();
            env.LampState(Detectors.hallwayToilet).Should().BeTrue();
        }

        // *[Confusion], ^[Resolved]
        //  ___________________________________________   __________________________
        // |        |                |                       |                      |
        // |        |                  1,2,3,4,5                                    |
        // |        |                |                       |                      |
        // |                         |___   ______           |                      |
        // |        |                |            |          |                      |
        // |        |                |            |          |                      |
        // |        |                |            |    6     |______________________|
        // |        |                |            |          |                      |
        // |        |                |            |                                 |
        // |        |                |            |____  ____|                      |
        // |        |                |            |          |                      |
        // |        |                |            |          |                      |
        // |        |                |            |          |                      |
        // |________|________________|____________|__________|______________________|
        [Fact]
        public void Leave_AfterMoveAroundRoom()
        {
            var confusionResolutionTime = TimeSpan.FromMilliseconds(5000);
            var servieConfig = GetServiceBuilder().WithConfusionResolutionTime(confusionResolutionTime).Build();
            using var env = GetEnviromentBuilder(servieConfig).WithMotions(new Dictionary<int, string>
            {
                { 500, Detectors.hallwayLivingRoom },
                { 2500, Detectors.hallwayLivingRoom },
                { 4500, Detectors.hallwayLivingRoom },
                { 7500, Detectors.hallwayLivingRoom },
                { 10500, Detectors.hallwayLivingRoom },
                { 11500, Detectors.hallwayToilet },
            }).Build();

            env.AdvanceToEnd(confusionResolutionTime + TimeSpan.FromSeconds(15));
        }

        [Fact]
        public void Leave_ConfusedFromRoom_ShouldNotResolveOtherConfusions()
        {
            var confusionResolutionTime = TimeSpan.FromMilliseconds(5000);
            var servieConfig = GetServiceBuilder().WithConfusionResolutionTime(confusionResolutionTime).Build();
            using var env = GetEnviromentBuilder(servieConfig).WithMotions(new Dictionary<int, string>
            {
                { 300, Detectors.bathroom },             // We have move in bathroom at start so future enter to bathroom is not sure and cannot cancel other confusions
                { 500, Detectors.hallwayLivingRoom },
                { 1000, Detectors.kitchen },
                { 1500, Detectors.hallwayToilet },
                { 1600, Detectors.bathroom }
            }).Build();

            env.AdvanceToEnd();

            env.LampState(Detectors.kitchen).Should().BeTrue();
            env.LampState(Detectors.hallwayLivingRoom).Should().BeTrue();
            env.LampState(Detectors.bathroom).Should().BeTrue();
            env.LampState(Detectors.hallwayToilet).Should().BeTrue();

            env.AdvanceToIndex(4, confusionResolutionTime, true);

            env.LampState(Detectors.kitchen).Should().BeFalse("Kitchen should be OFF after confusion resolution time");
            env.LampState(Detectors.hallwayLivingRoom).Should().BeTrue();
            env.LampState(Detectors.bathroom).Should().BeTrue();
            env.LampState(Detectors.hallwayToilet).Should().BeTrue();
        }

        [Fact]
        public void Move_OnSeparateRooms_ShouldTurnOnLight()
        {
            using var env = GetEnviromentBuilder(GetServiceBuilder().Build()).WithMotions(new Dictionary<int, string>
            {
                { 500, Detectors.toilet },
                { 1500, Detectors.kitchen },
                { 2000, Detectors.livingRoom }
            }).Build();

            env.AdvanceToEnd();

            env.LampState(Detectors.toilet).Should().BeTrue();
            env.LampState(Detectors.kitchen).Should().BeTrue();
            env.LampState(Detectors.livingRoom).Should().BeTrue();
        }

        [Fact]
        public async Task Move_Timeout_ShouldTurnOffAfterTimeout()
        {
            using var env = GetEnviromentBuilder(GetServiceBuilder().Build()).WithMotions(new Dictionary<int, string>
            {
                { 500, Detectors.kitchen }
            }).Build();

            var area = await env.Query<AreaDescriptor>(AreaDescriptorQuery.Create(Detectors.toilet));
            env.AdvanceToEnd();

            env.LampState(Detectors.kitchen).Should().BeTrue();
            env.AdvanceTo(area.TurnOffTimeout);
            env.LampState(Detectors.kitchen).Should().BeTrue();
            env.AdvanceTo(area.TurnOffTimeout, true);
            env.LampState(Detectors.kitchen).Should().BeFalse();
        }

        [Fact]
        public async Task Move_TurnOnAfterTurnOff_ShouldIncreaseTimeout()
        {
            using var env = GetEnviromentBuilder(GetServiceBuilder().Build()).WithMotions(new Dictionary<int, string>
            {
                { 500, Detectors.kitchen },
                { 12000, Detectors.kitchen },
            }).Build();

            env.AdvanceToEnd();
            var status = await env.Query<bool>(AutomationStateQuery.Create(Detectors.kitchen));
            status.Should().BeTrue();
        }

        [Fact]
        public void Timeout_WhenPassthrough_ShouldBeQuick()
        {
            var timeout = TimeSpan.FromSeconds(5);
            var moveTime = TimeSpan.FromSeconds(9);
            var serviceConfig = GetServiceBuilder();
            serviceConfig[Detectors.kitchen].WithTimeout(timeout);

            using var env = GetEnviromentBuilder(serviceConfig.Build()).WithRepeatedMotions(Detectors.kitchen, moveTime).Build();

            env.AdvanceToEnd();
            env.LampState(Detectors.kitchen).Should().BeTrue("After move we start counting and light should be on");
            env.AdvanceJustBefore(moveTime + timeout);
            env.LampState(Detectors.kitchen).Should().BeTrue();
            env.AdvanceTo(moveTime + timeout, true);
            env.LampState(Detectors.kitchen).Should().BeFalse();
        }

        [Fact]
        public void Timeout_AfterShortVisit_ShouldBeLonger()
        {
            var timeout = TimeSpan.FromSeconds(5);
            var moveTime = TimeSpan.FromSeconds(15);
            var serviceConfig = GetServiceBuilder();
            serviceConfig[Detectors.kitchen].WithTimeout(timeout);

            using var env = GetEnviromentBuilder(serviceConfig.Build()).WithRepeatedMotions(Detectors.kitchen, moveTime).Build();

            env.AdvanceToEnd();
            env.LampState(Detectors.kitchen).Should().BeTrue("After move we start counting and light should be on");
            env.AdvanceJustBefore(moveTime + 2 * timeout);
            env.LampState(Detectors.kitchen).Should().BeTrue();
            env.AdvanceTo(moveTime + 2 * timeout, true);
            env.LampState(Detectors.kitchen).Should().BeFalse();
        }

        [Fact]
        public void Timeout_AsfterLongVisit_ShouldBeLonger()
        {
            var timeout = TimeSpan.FromSeconds(5);
            var moveTime = TimeSpan.FromSeconds(75);
            var serviceConfig = GetServiceBuilder();
            serviceConfig[Detectors.kitchen].WithTimeout(timeout);

            using var env = GetEnviromentBuilder(serviceConfig.Build()).WithRepeatedMotions(Detectors.kitchen, moveTime).Build();

            env.AdvanceToEnd();
            env.LampState(Detectors.kitchen).Should().BeTrue("After move we start counting and light should be on");
            env.AdvanceJustBefore(moveTime + 3 * timeout);
            env.LampState(Detectors.kitchen).Should().BeTrue();
            env.AdvanceTo(moveTime + 3 * timeout, true);
            env.LampState(Detectors.kitchen).Should().BeFalse();
        }

        /// <summary>
        /// First not confused moving path should not be source of confusion for next path
        /// HT -> HL is eliminated because of that
        /// T -> HT -> K | L -> HL -> HT -> K
        /// </summary>
        [Fact]
        public async Task Confusion_NotConfusedVectors_ShouldNotBeConfused()
        {
            using var env = GetEnviromentBuilder(GetServiceBuilder().Build()).WithMotions(new Dictionary<int, string>
            {
                 // First path
                { 500, Detectors.toilet },
                { 1500, Detectors.hallwayToilet },
                { 2000, Detectors.kitchen },
                // Second path
                { 2500, Detectors.livingRoom },
                { 3000, Detectors.hallwayLivingRoom }
            }).Build();

            env.AdvanceToEnd();

            var status = await env.Query<MotionStatus>(MotionServiceStatusQuery.Create());
            status.NumberOfConfusions.Should().Be(0);
            env.LampState(Detectors.kitchen).Should().BeTrue();
        }

        //[Fact]
        //public void Confusion_MoveWithNoConfusion_ShouldTurnOfLightImmediately()
        //{
        //    new LightAutomationEnviromentBuilder(_context).WithServiceConfig(Default().Build()).WithMotions(new Dictionary<int, string>
        //    {
        //        { 1000, Detectors.livingRoomDetector },
        //        { 2000, Detectors.hallwayDetectorLivingRoom },
        //        { 3000, Detectors.hallwayDetectorToilet },
        //        { 4000, Detectors.kitchenDetector },
        //    }).Start();

        //    AdvanceJustAfter(TimeSpan.FromSeconds(2));

        //    env.LampState(Detectors.livingRoomDetector).Should().BeTrue();
        //    env.LampState(Detectors.hallwayDetectorLivingRoom).Should().BeTrue();
        //    //  scheduler.AdvanceJustAfter(TimeSpan.FromSeconds(3));
        //    //   Assert.AreEqual(false, lampDictionary[Detectors.livingRoomDetector].IsTurnedOn);
        //    //Assert.AreEqual(true, lampDictionary[Detectors.hallwayDetectorLivingRoom].IsTurnedOn);
        //    //Assert.AreEqual(true, lampDictionary[Detectors.hallwayDetectorToilet].IsTurnedOn);
        //    //scheduler.AdvanceJustAfter(TimeSpan.FromSeconds(4));
        //    //Assert.AreEqual(false, lampDictionary[Detectors.livingRoomDetector].IsTurnedOn);
        //    //Assert.AreEqual(false, lampDictionary[Detectors.hallwayDetectorLivingRoom].IsTurnedOn);
        //    //Assert.AreEqual(true, lampDictionary[Detectors.hallwayDetectorToilet].IsTurnedOn);
        //    //Assert.AreEqual(true, lampDictionary[Detectors.kitchenDetector].IsTurnedOn);
        //    //scheduler.AdvanceJustAfter(TimeSpan.FromSeconds(5));
        //    //Assert.AreEqual(false, lampDictionary[Detectors.livingRoomDetector].IsTurnedOn);
        //    //Assert.AreEqual(false, lampDictionary[Detectors.hallwayDetectorLivingRoom].IsTurnedOn);
        //    //Assert.AreEqual(false, lampDictionary[Detectors.hallwayDetectorToilet].IsTurnedOn);
        //    //Assert.AreEqual(true, lampDictionary[Detectors.kitchenDetector].IsTurnedOn);
        //}

        // TODO - Count should not decrease  when there is no exit from house
        // *[Confusion], ^[Resolved], P[PassThru], S[ShortVisit], L[LongVisit]
        //  ___________________________________________   __________________________
        // |        |                |                       |                      |
        // |        |           1S                                                  |
        // |        |                |                       |                      |
        // |                         |___   ______           |                      |
        // |        |                |            |          |                      |
        // |        |                |            |          |                      |
        // |        |                |            |          |______________________|
        // |        |                |            |          |                      |
        // |        |                |            |    4        3                   |
        // |        |                |            |____  ____|                      |
        // |        |                |            |          |                      |
        // |        |                |            |    2S    |                      |
        // |        |                |            |          |                      |
        // |________|________________|____________|__________|______________________|
        [Fact]
        public async Task Count_WhenMovingAround_ShouldGiveNumberOfPersons()
        {
            var servieConfig = GetServiceBuilder().WithConfusionResolutionTime(MotionDefaults.ConfusionResolutionTime).Build();
            using var env = GetEnviromentBuilder(servieConfig).WithRepeatedMotions(Detectors.livingRoom, TimeSpan.FromSeconds(15))
                                                                                        .WithRepeatedMotions(Detectors.toilet, TimeSpan.FromSeconds(15))
                                                                                        .WithMotions(new Dictionary<int, string> //Short moves should not be included
                                                                                        {
                                                                                            { 500, Detectors.kitchen },
                                                                                            { 1500, Detectors.hallwayToilet }
                                                                                        })
                                                                                        .Build();

            env.AdvanceToEnd();

            var status = await env.Query<MotionStatus>(MotionServiceStatusQuery.Create());

            status.NumberOfPersonsInHouse.Should().Be(2);
        }

        private LightAutomationEnviromentBuilder GetEnviromentBuilder(ServiceDTO serviceConfig) =>
            new LightAutomationEnviromentBuilder(serviceConfig, _useRavenDbLogs, _cleanLogsBeforeRun);

        /// <summary>
        /// Get predefined rooms configuration
        /// </summary>
        /// <returns></returns>
        private LightAutomationServiceBuilder GetServiceBuilder()
        {
            var builder = new LightAutomationServiceBuilder();

            builder.WithRoom(new RoomBuilder(Rooms.Hallway).WithDetector(Detectors.hallwayToilet, new List<string> { Detectors.hallwayLivingRoom, Detectors.kitchen, Detectors.staircaseDetector, Detectors.toilet, Detectors.badroomDetector })
                                                           .WithDetector(Detectors.hallwayLivingRoom, new List<string> { Detectors.livingRoom, Detectors.bathroom, Detectors.hallwayToilet }));
            builder.WithRoom(new RoomBuilder(Rooms.Badroom).WithDetector(Detectors.badroomDetector, new List<string> { Detectors.hallwayToilet }));
            builder.WithRoom(new RoomBuilder(Rooms.Balcony).WithDetector(Detectors.balconyDetector, new List<string> { Detectors.livingRoom }));
            builder.WithRoom(new RoomBuilder(Rooms.Bathroom).WithDetector(Detectors.bathroom, new List<string> { Detectors.hallwayLivingRoom }));
            builder.WithRoom(new RoomBuilder(Rooms.Kitchen).WithDetector(Detectors.kitchen, new List<string> { Detectors.hallwayToilet }));
            builder.WithRoom(new RoomBuilder(Rooms.Livingroom).WithDetector(Detectors.livingRoom, new List<string> { Detectors.balconyDetector, Detectors.hallwayLivingRoom }));
            builder.WithRoom(new RoomBuilder(Rooms.Staircase).WithDetector(Detectors.staircaseDetector, new List<string> { Detectors.hallwayToilet }));
            builder.WithRoom(new RoomBuilder(Rooms.Toilet).WithDetector(Detectors.toilet, new List<string> { Detectors.hallwayToilet }).WithProperty(MotionProperties.MaxPersonCapacity, 1));

            return builder;
        }
    }
}