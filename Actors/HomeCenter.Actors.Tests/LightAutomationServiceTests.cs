using FluentAssertions;
using HomeCenter.Model.Core;
using HomeCenter.Services.Configuration.DTO;
using HomeCenter.Services.MotionService.Commands;
using HomeCenter.Services.MotionService.Model;
using Microsoft.Reactive.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HomeCenter.Services.MotionService.Tests
{
    //                                             STAIRCASE [S]<1+>
    //  ________________________________________<_    __________________________
    // |        |                |                       |                      |
    // |        |                  [HL]<1+>  HALLWAY     |                      |
    // |   B    |                |<            [H]<1+>   |<                     |
    // |   A                     |___   ______           |       BADROOM        |
    // |   L    |                |            |                    [D]<1+>      |
    // |   C    |                |            |          |                      |
    // |   O    |                |            |          |______________________|
    // |   N    |   LIVINGROOM  >|            |          |<                     |
    // |   Y    |      [L]<1+>   |  BATHROOM  | [HT]<1+> |                      |
    // |        |                |   [B]<1+> >|___v  ____|                      |
    // | [Y]<1+>|                |            |          |       KITCHEN        |
    // |        |                |            |  TOILET  |         [K]<1+>      |
    // |        |                |            |  [T]<1>  |                      |
    // |_______v|________________|____________|_____v____|______________________|
    //
    // LEGEND: v/< - Motion Detector
    //         <x> - Max number of persons

    [TestClass]
    public class LightAutomationServiceTests : ReactiveTest
    {
        [TestMethod]
        public void WorkingTime_DuringDaylight_ShuldPowerOnDayLights()
        {
            var servieConfig = GetLightAutomationServiceBuilder().WithWorkingTime(WorkingTime.DayLight).Build();
            using var env = GetLightAutomationEnviromentBuilder(servieConfig).WithMotions(new Dictionary<int, string>
            {
                { 500, Detectors.toiletDetector },
                { 1500, Detectors.kitchenDetector },
                { 2000, Detectors.livingRoomDetector }
            }).Build();

            SystemTime.Set(TimeSpan.FromHours(12));

            env.AdvanceToEnd();

            env.LampState(Detectors.toiletDetector).Should().BeTrue();
            env.LampState(Detectors.kitchenDetector).Should().BeTrue();
            env.LampState(Detectors.livingRoomDetector).Should().BeTrue();
        }

        [TestMethod]
        public void WorkingTime_AfterDusk_ShouldNotPowerOnDayLight()
        {
            var servieConfig = GetLightAutomationServiceBuilder().WithWorkingTime(WorkingTime.DayLight).Build();
            using var env = GetLightAutomationEnviromentBuilder(servieConfig).WithMotions(new Dictionary<int, string>
            {
                { 500, Detectors.toiletDetector },
                { 1500, Detectors.kitchenDetector },
                { 2000, Detectors.livingRoomDetector }
            }).Build();

            SystemTime.Set(TimeSpan.FromHours(21));

            env.AdvanceToEnd();

            env.LampState(Detectors.toiletDetector).Should().BeFalse();
            env.LampState(Detectors.kitchenDetector).Should().BeFalse();
            env.LampState(Detectors.livingRoomDetector).Should().BeFalse();
        }

        [TestMethod]
        public void WorkingTime_DuringDaylight_ShuldNotPowerOnNightLight()
        {
            var servieConfig = GetLightAutomationServiceBuilder().WithWorkingTime(WorkingTime.AfterDusk).Build();
            using var env = GetLightAutomationEnviromentBuilder(servieConfig).WithMotions(new Dictionary<int, string>
            {
                { 500, Detectors.toiletDetector },
                { 1500, Detectors.kitchenDetector },
                { 2000, Detectors.livingRoomDetector }
            }).Build();

            SystemTime.Set(TimeSpan.FromHours(12));

            env.AdvanceToEnd();

            env.LampState(Detectors.toiletDetector).Should().BeFalse();
            env.LampState(Detectors.kitchenDetector).Should().BeFalse();
            env.LampState(Detectors.livingRoomDetector).Should().BeFalse();
        }

        [TestMethod]
        public async Task Automation_WhenDisabled_ShouldNoturnOnLights()
        {
            using var env = GetLightAutomationEnviromentBuilder(GetLightAutomationServiceBuilder().Build()).WithMotions(new Dictionary<int, string>
            {
                { 500, Detectors.toiletDetector }
            }).Build();

            env.SendCommand(DisableAutomationCommand.Create(Detectors.toiletDetector));
            env.AdvanceToEnd();

            env.LampState(Detectors.toiletDetector).Should().BeFalse();
            var automationState = await env.Query<bool>(AutomationStateQuery.Create(Detectors.toiletDetector));
            automationState.Should().BeFalse();
        }

        [TestMethod]
        public void Automation_WhenReEnabled_ShouldTurnOnLights()
        {
            using var env = GetLightAutomationEnviromentBuilder(GetLightAutomationServiceBuilder().Build()).WithMotions(new Dictionary<int, string>
            {
                { 500, Detectors.toiletDetector },
                { 2500, Detectors.toiletDetector }
            }).Build();

            env.SendCommand(DisableAutomationCommand.Create(Detectors.toiletDetector));
            env.RunAfterFirstMove(_ => env.SendCommand(EnableAutomationCommand.Create(Detectors.toiletDetector)));

            env.LampState(Detectors.toiletDetector).Should().BeFalse();
            env.AdvanceToEnd();
            env.LampState(Detectors.toiletDetector).Should().BeTrue();
        }

        [TestMethod]
        public void Leave_FromOnePeopleRoomWithNoConfusion_ShouldTurnOffLightImmediately()
        {
            using var env = GetLightAutomationEnviromentBuilder(GetLightAutomationServiceBuilder().Build()).WithMotions(new Dictionary<int, string>
            {
                { 500, Detectors.toiletDetector },
                { 1500, Detectors.hallwayDetectorToilet }
            }).Build();

            env.AdvanceJustAfterEnd();

            env.LampState(Detectors.toiletDetector).Should().BeFalse();
        }

        [TestMethod]
        public void Leave_FromOnePeopleRoomWithConfusion_ShouldTurnOffAfterResolvingConfusion()
        {
            var confusionResolutionTime = TimeSpan.FromMilliseconds(5000);
            var servieConfig = GetLightAutomationServiceBuilder().WithConfusionResolutionTime(confusionResolutionTime).Build();
            using var env = GetLightAutomationEnviromentBuilder(servieConfig).WithMotions(new Dictionary<int, string>
            {
                { 500, Detectors.toiletDetector },
                { 1000, Detectors.kitchenDetector },
                { 1500, Detectors.hallwayDetectorToilet },
                { 2000, Detectors.hallwayDetectorLivingRoom },
                { 3000, Detectors.kitchenDetector },
            }).Build();

            env.AdvanceJustAfterRoundUp(confusionResolutionTime + env.GetMotionTime(2));

            env.LampState(Detectors.toiletDetector).Should().BeFalse();
            env.LampState(Detectors.kitchenDetector).Should().BeTrue();
        }

        [TestMethod]
        public void Leave_FromSeparateRoomsIntoOne_ShouldTurnOffLightInBoth()
        {
            var confusionResolutionTime = TimeSpan.FromMilliseconds(5000);
            var servieConfig = GetLightAutomationServiceBuilder().WithConfusionResolutionTime(confusionResolutionTime).Build();
            using var env = GetLightAutomationEnviromentBuilder(servieConfig).WithMotions(new Dictionary<int, string>
            {
                { 500, Detectors.badroomDetector },
                { 1000, Detectors.hallwayDetectorLivingRoom },
                { 1500, Detectors.hallwayDetectorToilet }
            }).Build();

            env.AdvanceJustAfterRoundUp(env.GetMotionTime(2) + confusionResolutionTime);

            env.LampState(Detectors.badroomDetector).Should().BeFalse();
            env.LampState(Detectors.hallwayDetectorLivingRoom).Should().BeFalse();
            env.LampState(Detectors.hallwayDetectorToilet).Should().BeTrue();
        }

        [TestMethod]
        public void Leave_ToOtherRoom_ShouldSpeedUpResolutionInNeighbor()
        {
            var confusionResolutionTime = TimeSpan.FromMilliseconds(5000);
            var servieConfig = GetLightAutomationServiceBuilder().WithConfusionResolutionTime(confusionResolutionTime).Build();
            using var env = GetLightAutomationEnviromentBuilder(servieConfig).WithMotions(new Dictionary<int, string>
            {
                { 500, Detectors.hallwayDetectorLivingRoom },
                { 1000, Detectors.toiletDetector },
                { 1500, Detectors.hallwayDetectorToilet },
                { 1600, Detectors.bathroomDetector }     // This move give us proper move to bathroom so other confusions can resolve faster because this decrease their probability
            }).Build();

            env.AdvanceJustAfterRoundUp(env.GetLastMotionTime() + confusionResolutionTime / 2); // We should resolve confusion 2x faster

            env.LampState(Detectors.toiletDetector).Should().BeFalse();
            env.LampState(Detectors.hallwayDetectorLivingRoom).Should().BeFalse();
            env.LampState(Detectors.bathroomDetector).Should().BeTrue();
            env.LampState(Detectors.hallwayDetectorToilet).Should().BeTrue();
        }

        [TestMethod]
        public void Leave_ToOtherRoomAfterLongerBeeing_Should()
        {
            var confusionResolutionTime = TimeSpan.FromMilliseconds(5000);
            var servieConfig = GetLightAutomationServiceBuilder().WithConfusionResolutionTime(confusionResolutionTime).Build();
            using var env = GetLightAutomationEnviromentBuilder(servieConfig).WithMotions(new Dictionary<int, string>
            {
                { 500, Detectors.hallwayDetectorLivingRoom },
                { 2500, Detectors.hallwayDetectorLivingRoom },
                { 4500, Detectors.hallwayDetectorLivingRoom },
                { 7500, Detectors.hallwayDetectorLivingRoom },
                { 10500, Detectors.hallwayDetectorLivingRoom },  // We were in this room longer so when leave there will be longer time out
                { 11000, Detectors.toiletDetector },
                { 11500, Detectors.hallwayDetectorToilet },
                { 11600, Detectors.bathroomDetector }
            }).Build();

            env.AdvanceJustAfterRoundUp(env.GetLastMotionTime() + confusionResolutionTime + TimeSpan.FromSeconds(5));

            //  env.LampState(Detectors.toiletDetector).Should().BeFalse();
            env.LampState(Detectors.hallwayDetectorLivingRoom).Should().BeFalse();
            //env.LampState(Detectors.bathroomDetector).Should().BeTrue();
            //env.LampState(Detectors.hallwayDetectorToilet).Should().BeTrue();
        }

        [TestMethod]
        public void Leave_AfterMoveAroundRoom()
        {
            var confusionResolutionTime = TimeSpan.FromMilliseconds(5000);
            var servieConfig = GetLightAutomationServiceBuilder().WithConfusionResolutionTime(confusionResolutionTime).Build();
            using var env = GetLightAutomationEnviromentBuilder(servieConfig).WithMotions(new Dictionary<int, string>
            {
                { 500, Detectors.hallwayDetectorLivingRoom },
                { 2500, Detectors.hallwayDetectorLivingRoom },
                { 4500, Detectors.hallwayDetectorLivingRoom },
                { 7500, Detectors.hallwayDetectorLivingRoom },
                { 10500, Detectors.hallwayDetectorLivingRoom },
                { 11500, Detectors.hallwayDetectorToilet },
            }).Build();

            env.AdvanceJustAfterRoundUp(env.GetLastMotionTime() + confusionResolutionTime + TimeSpan.FromSeconds(15));
        }

        [TestMethod]
        public void Leave_ConfusedFromRoom_ShouldNotResolveOtherConfusions()
        {
            var confusionResolutionTime = TimeSpan.FromMilliseconds(5000);
            var servieConfig = GetLightAutomationServiceBuilder().WithConfusionResolutionTime(confusionResolutionTime).Build();
            using var env = GetLightAutomationEnviromentBuilder(servieConfig).WithMotions(new Dictionary<int, string>
            {
                { 300, Detectors.bathroomDetector },             // We have move in bathroom at start so future enter to bathroom is not sure and cannot cancel other confusions
                { 500, Detectors.hallwayDetectorLivingRoom },
                { 1000, Detectors.kitchenDetector },
                { 1500, Detectors.hallwayDetectorToilet },
                { 1600, Detectors.bathroomDetector }
            }).Build();

            env.AdvanceJustAfterEnd();

            env.LampState(Detectors.kitchenDetector).Should().BeTrue();
            env.LampState(Detectors.hallwayDetectorLivingRoom).Should().BeTrue();
            env.LampState(Detectors.bathroomDetector).Should().BeTrue();
            env.LampState(Detectors.hallwayDetectorToilet).Should().BeTrue();

            env.AdvanceJustAfterRoundUp(confusionResolutionTime + env.GetMotionTime(4));

            env.LampState(Detectors.kitchenDetector).Should().BeFalse("Kitchen should be OFF after confusion resolution time");
            env.LampState(Detectors.hallwayDetectorLivingRoom).Should().BeTrue();
            env.LampState(Detectors.bathroomDetector).Should().BeTrue();
            env.LampState(Detectors.hallwayDetectorToilet).Should().BeTrue();
        }

        [TestMethod]
        public void Move_OnSeparateRooms_ShouldTurnOnLight()
        {
            using var env = GetLightAutomationEnviromentBuilder(GetLightAutomationServiceBuilder().Build()).WithMotions(new Dictionary<int, string>
            {
                { 500, Detectors.toiletDetector },
                { 1500, Detectors.kitchenDetector },
                { 2000, Detectors.livingRoomDetector }
            }).Build();

            env.AdvanceToEnd();

            env.LampState(Detectors.toiletDetector).Should().BeTrue();
            env.LampState(Detectors.kitchenDetector).Should().BeTrue();
            env.LampState(Detectors.livingRoomDetector).Should().BeTrue();
        }

        [TestMethod]
        public async Task Move_Timeout_ShouldTurnOffAfterTimeout()
        {
            using var env = GetLightAutomationEnviromentBuilder(GetLightAutomationServiceBuilder().Build()).WithMotions(new Dictionary<int, string>
            {
                { 500, Detectors.kitchenDetector }
            }).Build();

            var area = await env.Query<AreaDescriptor>(AreaDescriptorQuery.Create(Detectors.toiletDetector));
            env.AdvanceJustAfterEnd();

            env.LampState(Detectors.kitchenDetector).Should().BeTrue();
            env.AdvanceTo(area.TurnOffTimeout);
            env.LampState(Detectors.kitchenDetector).Should().BeTrue();
            env.AdvanceJustAfter(area.TurnOffTimeout);
            env.LampState(Detectors.kitchenDetector).Should().BeFalse();
        }

        [TestMethod]
        public async Task Move_TurnOnAfterTurnOff_ShouldIncreaseTimeout()
        {
            using var env = GetLightAutomationEnviromentBuilder(GetLightAutomationServiceBuilder().Build()).WithMotions(new Dictionary<int, string>
            {
                { 500, Detectors.kitchenDetector },
                { 12000, Detectors.kitchenDetector },
            }).Build();

            env.AdvanceJustAfterEnd();
            var status = await env.Query<bool>(AutomationStateQuery.Create(Detectors.kitchenDetector));
            status.Should().BeTrue();
        }

        [TestMethod]
        public void Timeout_WhenPassthrough_ShouldBeQuick()
        {
            var timeout = TimeSpan.FromSeconds(5);
            var moveTime = TimeSpan.FromSeconds(9);
            var serviceConfig = GetLightAutomationServiceBuilder();
            serviceConfig[Detectors.kitchenDetector].WithTimeout(timeout);

            using var env = GetLightAutomationEnviromentBuilder(serviceConfig.Build()).WithRepeatedMotions(Detectors.kitchenDetector, moveTime).Build();

            env.AdvanceToEnd();
            env.LampState(Detectors.kitchenDetector).Should().BeTrue("After move we start counting and light should be on");
            env.AdvanceJustBefore(moveTime + timeout);
            env.LampState(Detectors.kitchenDetector).Should().BeTrue();
            env.AdvanceJustAfter(moveTime + timeout);
            env.LampState(Detectors.kitchenDetector).Should().BeFalse();
        }

        [TestMethod]
        public void Timeout_AfterShortVisit_ShouldBeLonger()
        {
            var timeout = TimeSpan.FromSeconds(5);
            var moveTime = TimeSpan.FromSeconds(15);
            var serviceConfig = GetLightAutomationServiceBuilder();
            serviceConfig[Detectors.kitchenDetector].WithTimeout(timeout);

            using var env = GetLightAutomationEnviromentBuilder(serviceConfig.Build()).WithRepeatedMotions(Detectors.kitchenDetector, moveTime).Build();

            env.AdvanceToEnd();
            env.LampState(Detectors.kitchenDetector).Should().BeTrue("After move we start counting and light should be on");
            env.AdvanceJustBefore(moveTime + 2 * timeout);
            env.LampState(Detectors.kitchenDetector).Should().BeTrue();
            env.AdvanceJustAfter(moveTime + 2 * timeout);
            env.LampState(Detectors.kitchenDetector).Should().BeFalse();
        }

        [TestMethod]
        public void Timeout_AsfterLongVisit_ShouldBeLonger()
        {
            var timeout = TimeSpan.FromSeconds(5);
            var moveTime = TimeSpan.FromSeconds(75);
            var serviceConfig = GetLightAutomationServiceBuilder();
            serviceConfig[Detectors.kitchenDetector].WithTimeout(timeout);

            using var env = GetLightAutomationEnviromentBuilder(serviceConfig.Build()).WithRepeatedMotions(Detectors.kitchenDetector, moveTime).Build();

            env.AdvanceToEnd();
            env.LampState(Detectors.kitchenDetector).Should().BeTrue("After move we start counting and light should be on");
            env.AdvanceJustBefore(moveTime + 3 * timeout);
            env.LampState(Detectors.kitchenDetector).Should().BeTrue();
            env.AdvanceJustAfter(moveTime + 3 * timeout);
            env.LampState(Detectors.kitchenDetector).Should().BeFalse();
        }

        /// <summary>
        /// First not confused moving path should not be source of confusion for next path
        /// HT -> HL is eliminated because of that
        /// T -> HT -> K | L -> HL -> HT -> K
        /// </summary>
        [TestMethod]
        public async Task Confusion_NotConfusedVectors_ShouldNotBeConfused()
        {
            using var env = GetLightAutomationEnviromentBuilder(GetLightAutomationServiceBuilder().Build()).WithMotions(new Dictionary<int, string>
            {
                 // First path
                { 500, Detectors.toiletDetector },
                { 1500, Detectors.hallwayDetectorToilet },
                { 2000, Detectors.kitchenDetector },
                // Second path
                { 2500, Detectors.livingRoomDetector },
                { 3000, Detectors.hallwayDetectorLivingRoom }
            }).Build();

            env.AdvanceJustAfterEnd();

            var status = await env.Query<MotionStatus>(MotionServiceStatusQuery.Create());
            status.NumberOfConfusions.Should().Be(0);
            env.LampState(Detectors.kitchenDetector).Should().BeTrue();
        }

        //[TestMethod]
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

        [TestMethod]
        public async Task Count_WhenCrossPassing_ShouldCountNumberOfPeople()
        {
            using var env = GetLightAutomationEnviromentBuilder(GetLightAutomationServiceBuilder().Build()).WithMotions(new Dictionary<int, string>
            {
                { 500, Detectors.kitchenDetector },
                { 501, Detectors.livingRoomDetector },

                { 1500, Detectors.hallwayDetectorToilet },
                { 1501, Detectors.hallwayDetectorLivingRoom },

                //OnNext(Time.Tics(2000), new MotionEnvelope(HallwaylivingRoomDetector)), <- Undetected due motion detectors lag after previous move
                //OnNext(Time.Tics(2000), new MotionEnvelope(HallwaytoiletDetector)),     <- Undetected due motion detectors lag after previous move

                { 3000, Detectors.livingRoomDetector },
                { 3001, Detectors.kitchenDetector }
            }).Build();

            env.AdvanceJustAfterEnd();

            var status = await env.Query<MotionStatus>(MotionServiceStatusQuery.Create());
            var kitchen = await env.Query<int>(NumberOfPeopleQuery.Create(Detectors.kitchenDetector));
            var living = await env.Query<int>(NumberOfPeopleQuery.Create(Detectors.livingRoomDetector));

            kitchen.Should().Be(1);
            living.Should().Be(1);
            //Assert.AreEqual(2, status.NumberOfPersonsInHouse); //TODO
        }

        //[TestMethod]
        //public async Task Count_TwoVectorsToSameRoom()
        //{
        //    var servieConfig = Default().WithConfusionResolutionTime(TimeSpan.FromMilliseconds(5000)).Build();
        //    new LightAutomationEnviromentBuilder(_context).WithServiceConfig(servieConfig).WithMotions(new Dictionary<int, string>
        //    {
        //        { 500, Detectors.toiletDetector },
        //        { 1500, Detectors.hallwayDetectorToilet },
        //        { 2000, Detectors.kitchenDetector },
        //        { 2500, Detectors.livingRoomDetector },
        //        { 3000, Detectors.hallwayDetectorLivingRoom },
        //        { 3500, Detectors.hallwayDetectorToilet },
        //        { 4000, Detectors.kitchenDetector },
        //    }).Start();

        //    AdvanceTo(TimeSpan.FromMilliseconds(11000));

        //    var num = await Query<int>(NumberOfPeopleQuery.Create(Detectors.kitchenDetector));
        //    num.Should().Be(2);
        //}

        //[TestMethod]
        //public async Task MoveCloseToRoomWithOtherPersonShouldConfuzeVectorsNearThatRoom()
        //{
        //    var (motionEvents, scheduler, lampDictionary) = new LightAutomationServiceBuilder(_context).WithMotion
        //    (
        //        //L->HL->HT->K vs move in B
        //        OnNext(Time.Tics(1000), new MotionEnvelope(Detectors.livingRoomDetector)),
        //        OnNext(Time.Tics(1500), new MotionEnvelope(Detectors.bathroomDetector)),
        //        OnNext(Time.Tics(2000), new MotionEnvelope(Detectors.hallwayDetectorLivingRoom)),
        //        OnNext(Time.Tics(2900), new MotionEnvelope(Detectors.bathroomDetector)),
        //        OnNext(Time.Tics(3000), new MotionEnvelope(Detectors.hallwayDetectorToilet)),
        //        OnNext(Time.Tics(4000), new MotionEnvelope(Detectors.kitchenDetector)),
        //        OnNext(Time.Tics(4100), new MotionEnvelope(Detectors.bathroomDetector))
        //    ).Build();

        //    scheduler.AdvanceJustAfter(TimeSpan.FromSeconds(9));

        //    var status = await _context.Query<MotionStatus>(MotionServiceStatusQuery.Create());
        //    Assert.AreEqual(2, status.NumberOfConfusions);
        //    Assert.AreEqual(true, lampDictionary[Detectors.bathroomDetector].IsTurnedOn);
        //    Assert.AreEqual(true, lampDictionary[Detectors.kitchenDetector].IsTurnedOn);
        //    Assert.AreEqual(false, lampDictionary[Detectors.livingRoomDetector].IsTurnedOn);
        //    Assert.AreEqual(false, lampDictionary[Detectors.hallwayDetectorLivingRoom].IsTurnedOn);
        //    Assert.AreEqual(false, lampDictionary[Detectors.hallwayDetectorToilet].IsTurnedOn);
        //}

        //[TestMethod]
        //public async Task AnalyzeMoveShouldCountPeopleNumberInHouse()
        //{
        //    var (motionEvents, scheduler, lampDictionary) = new LightAutomationServiceBuilder(_context).WithConfusionResolutionTime(TimeSpan.FromMilliseconds(5000))
        //                                                                                   .WithMotion
        //    (
        //      OnNext(Time.Tics(500), new MotionEnvelope(Detectors.toiletDetector)),
        //      OnNext(Time.Tics(1500), new MotionEnvelope(Detectors.hallwayDetectorToilet)),
        //      OnNext(Time.Tics(2000), new MotionEnvelope(Detectors.kitchenDetector)),
        //      OnNext(Time.Tics(2500), new MotionEnvelope(Detectors.livingRoomDetector)),
        //      OnNext(Time.Tics(3000), new MotionEnvelope(Detectors.hallwayDetectorLivingRoom)),
        //      OnNext(Time.Tics(3500), new MotionEnvelope(Detectors.hallwayDetectorToilet)),
        //      OnNext(Time.Tics(4000), new MotionEnvelope(Detectors.kitchenDetector))
        //    ).Build();

        //    scheduler.AdvanceTo(TimeSpan.FromMilliseconds(15000));

        //    var status = await _context.Query<MotionStatus>(MotionServiceStatusQuery.Create());
        //    Assert.AreEqual(2, status.NumberOfPersonsInHouse);
        //}

        //[TestMethod]
        //public async Task Move_SplitFromOneRoom_ShouldCountPeople()
        //{
        //    new LightAutomationEnviromentBuilder(_context).WithServiceConfig(DefaultServiceConfig().Build()).WithMotions(new Dictionary<int, string>
        //    {
        //        { 1000, Detectors.livingRoomDetector },
        //        { 2000, Detectors.hallwayDetectorLivingRoom },
        //        { 2900, Detectors.bathroomDetector },
        //        { 3000, Detectors.hallwayDetectorToilet },
        //        { 4000, Detectors.kitchenDetector },
        //    }).Start();

        //    AdvanceJustAfter(TimeSpan.FromSeconds(4));

        //    var status = await Query<MotionStatus>(MotionServiceStatusQuery.Create());
        //    var numKitchen = await Query<int>(NumberOfPeopleQuery.Create(Detectors.kitchenDetector));
        //    var numBathroom = await Query<int>(NumberOfPeopleQuery.Create(Detectors.bathroomDetector));

        //    numKitchen.Should().Be(1);
        //    numBathroom.Should().Be(1);

        //    IsTurnedOn(Detectors.kitchenDetector).Should().BeTrue();
        //    IsTurnedOn(Detectors.bathroomDetector).Should().BeTrue();
        //    IsTurnedOn(Detectors.hallwayDetectorLivingRoom).Should().BeFalse();
        //    //  Assert.AreEqual(2, status.NumberOfPersonsInHouse);
        //    //Assert.AreEqual(false, lampDictionary[Detectors.livingRoomDetector].IsTurnedOn);
        //    AdvanceJustAfter(TimeSpan.FromSeconds(5));
        //    IsTurnedOn(Detectors.hallwayDetectorToilet).Should().BeFalse();
        //}

        private LightAutomationEnviromentBuilder GetLightAutomationEnviromentBuilder(ServiceDTO serviceConfig) => new LightAutomationEnviromentBuilder(serviceConfig, true);

        /// <summary>
        /// Get predefinied rooms configuration
        /// </summary>
        /// <returns></returns>
        private LightAutomationServiceBuilder GetLightAutomationServiceBuilder()
        {
            var builder = new LightAutomationServiceBuilder();

            builder.WithRoom(new RoomBuilder(Rooms.Hallway).WithDetector(Detectors.hallwayDetectorToilet, new List<string> { Detectors.hallwayDetectorLivingRoom, Detectors.kitchenDetector, Detectors.staircaseDetector, Detectors.toiletDetector, Detectors.badroomDetector })
                                                           .WithDetector(Detectors.hallwayDetectorLivingRoom, new List<string> { Detectors.livingRoomDetector, Detectors.bathroomDetector, Detectors.hallwayDetectorToilet }));
            builder.WithRoom(new RoomBuilder(Rooms.Badroom).WithDetector(Detectors.badroomDetector, new List<string> { Detectors.hallwayDetectorToilet }));
            builder.WithRoom(new RoomBuilder(Rooms.Balcony).WithDetector(Detectors.balconyDetector, new List<string> { Detectors.livingRoomDetector }));
            builder.WithRoom(new RoomBuilder(Rooms.Bathroom).WithDetector(Detectors.bathroomDetector, new List<string> { Detectors.hallwayDetectorLivingRoom }));
            builder.WithRoom(new RoomBuilder(Rooms.Kitchen).WithDetector(Detectors.kitchenDetector, new List<string> { Detectors.hallwayDetectorToilet }));
            builder.WithRoom(new RoomBuilder(Rooms.Livingroom).WithDetector(Detectors.livingRoomDetector, new List<string> { Detectors.balconyDetector, Detectors.hallwayDetectorLivingRoom }));
            builder.WithRoom(new RoomBuilder(Rooms.Staircase).WithDetector(Detectors.staircaseDetector, new List<string> { Detectors.hallwayDetectorToilet }));
            builder.WithRoom(new RoomBuilder(Rooms.Toilet).WithDetector(Detectors.toiletDetector, new List<string> { Detectors.hallwayDetectorToilet }).WithProperty(MotionProperties.MaxPersonCapacity, 1));

            return builder;
        }
    }
}