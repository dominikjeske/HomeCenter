using FluentAssertions;
using HomeCenter.Model.Core;
using HomeCenter.Services.MotionService.Commands;
using HomeCenter.Services.MotionService.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HomeCenter.Services.MotionService.Tests
{
    //                                             STAIRCASE [S]
    //  ________________________________________<_    __________________________
    // |        |                |                       |                      |
    // |        |                  [HL]      HALLWAY     |                      |
    // |   B    |                |<            [H]       |<                     |
    // |   A                     |___   ______           |       BADROOM        |
    // |   L    |                |            |                    [D]          |
    // |   C    |                |            |          |                      |
    // |   O    |                |            |          |______________________|
    // |   N    |   LIVINGROOM  >|            |          |<                     |
    // |   Y    |      [L]       |  BATHROOM  |   [HT]                          |
    // |        |                |     [B]   >|___v  ____|                      |
    // |  [Y]   |                |            |          |       KITCHEN        |
    // |        |                |            |  TOILET  |         [K]          |
    // |        |                |            |    [T]   |                      |
    // |_______v|________________|____________|_____v____|______________________|
    //
    // LEGEND: v/< - Motion Detector

    [TestClass]
    public class LightAutomationServiceTests : ReactiveActorTests
    {
        [TestMethod]
        public void WorkingTime_DuringDaylight_ShuldPowerOnDayLights()
        {
            var servieConfig = Default().WithWorkingTime(WorkingTime.DayLight).Build();
            new LightAutomationEnviromentBuilder(_context).WithServiceConfig(servieConfig).WithMotions(new Dictionary<int, string>
            {
                { 500, Detectors.toiletDetector },
                { 1500, Detectors.kitchenDetector },
                { 2000, Detectors.livingRoomDetector }
            }).Start();

            SystemTime.Set(TimeSpan.FromHours(12));

            AdvanceToEnd();

            LampState(Detectors.toiletDetector).Should().BeTrue();
            LampState(Detectors.kitchenDetector).Should().BeTrue();
            LampState(Detectors.livingRoomDetector).Should().BeTrue();
        }

        [TestMethod]
        public void WorkingTime_AfterDusk_ShouldNotPowerOnDayLight()
        {
            var servieConfig = Default().WithWorkingTime(WorkingTime.DayLight).Build();
            new LightAutomationEnviromentBuilder(_context).WithServiceConfig(servieConfig).WithMotions(new Dictionary<int, string>
            {
                { 500, Detectors.toiletDetector },
                { 1500, Detectors.kitchenDetector },
                { 2000, Detectors.livingRoomDetector }
            }).Start();

            SystemTime.Set(TimeSpan.FromHours(21));

            AdvanceToEnd();

            LampState(Detectors.toiletDetector).Should().BeFalse();
            LampState(Detectors.kitchenDetector).Should().BeFalse();
            LampState(Detectors.livingRoomDetector).Should().BeFalse();
        }

        [TestMethod]
        public void WorkingTime_DuringDaylight_ShuldNotPowerOnNightLight()
        {
            var servieConfig = Default().WithWorkingTime(WorkingTime.AfterDusk).Build();
            new LightAutomationEnviromentBuilder(_context).WithServiceConfig(servieConfig).WithMotions(new Dictionary<int, string>
            {
                { 500, Detectors.toiletDetector },
                { 1500, Detectors.kitchenDetector },
                { 2000, Detectors.livingRoomDetector }
            }).Start();

            SystemTime.Set(TimeSpan.FromHours(12));

            AdvanceToEnd();

            LampState(Detectors.toiletDetector).Should().BeFalse();
            LampState(Detectors.kitchenDetector).Should().BeFalse();
            LampState(Detectors.livingRoomDetector).Should().BeFalse();
        }

        [TestMethod]
        public async Task Automation_WhenAutomationDisabled_ShouldNoturnOnLights()
        {
            new LightAutomationEnviromentBuilder(_context).WithServiceConfig(Default().Build()).WithMotions(new Dictionary<int, string>
            {
                { 500, Detectors.toiletDetector }
            }).Start();

            SendCommand(DisableAutomationCommand.Create(Detectors.toiletDetector));
            AdvanceToEnd();

            LampState(Detectors.toiletDetector).Should().BeFalse();
            var automationState = await Query<bool>(AutomationStateQuery.Create(Detectors.toiletDetector));
            automationState.Should().BeFalse();
        }

        [TestMethod]
        public void Automation_WhenReEnableAutomation_ShouldTurnOnLights()
        {
            new LightAutomationEnviromentBuilder(_context).WithServiceConfig(Default().Build()).WithMotions(new Dictionary<int, string>
            {
                { 500, Detectors.toiletDetector },
                { 2500, Detectors.toiletDetector }
            }).Start();

            SendCommand(DisableAutomationCommand.Create(Detectors.toiletDetector));
            RunAfterFirstMove(_ => SendCommand(EnableAutomationCommand.Create(Detectors.toiletDetector)));
            AdvanceToEnd();

            LampState(Detectors.toiletDetector).Should().BeTrue();
        }

        [TestMethod]
        public void Leave_FromOnePeopleRoomWithNoConfusion_ShouldTurnOffLightImmediately()
        {
            new LightAutomationEnviromentBuilder(_context).WithServiceConfig(Default().Build()).WithMotions(new Dictionary<int, string>
            {
                { 500, Detectors.toiletDetector },
                { 1500, Detectors.hallwayDetectorToilet }
            }).Start();

            AdvanceJustAfterEnd();

            LampState(Detectors.toiletDetector).Should().BeFalse();
        }

        [TestMethod]
        public void Leave_FromOnePeopleRoomWithConfusion_ShouldTurnOffAfterResolvingConfusion()
        {
            var confusionResolutionTime = TimeSpan.FromMilliseconds(5000);
            var servieConfig = Default().WithConfusionResolutionTime(confusionResolutionTime).Build();
            new LightAutomationEnviromentBuilder(_context).WithServiceConfig(servieConfig).WithMotions(new Dictionary<int, string>
            {
                { 500, Detectors.toiletDetector },
                { 1000, Detectors.kitchenDetector },
                { 1500, Detectors.hallwayDetectorToilet },
                { 2000, Detectors.hallwayDetectorLivingRoom },
                { 3000, Detectors.kitchenDetector },
            }).Start();

            AdvanceTo(confusionResolutionTime + TimeSpan.FromMilliseconds(1500));

            LampState(Detectors.toiletDetector).Should().BeFalse();
        }

        [TestMethod]
        public void Leave_FromRoomWithConfusion_ShouldTurnOffLightAfterConfusionResolved()
        {
            new LightAutomationEnviromentBuilder(_context).WithServiceConfig(Default().Build()).WithMotions(new Dictionary<int, string>
            {
                { 500, Detectors.kitchenDetector },
                { 1500, Detectors.hallwayDetectorToilet }
            }).Start();

            AdvanceJustAfterEnd();

            LampState(Detectors.kitchenDetector).Should().BeTrue();
            AdvanceTo(Time.Tics(2500));
            LampState(Detectors.kitchenDetector).Should().BeFalse();
        }

        [TestMethod]
        public void Move_OnSeparateRooms_ShouldTurnOnLight()
        {
            new LightAutomationEnviromentBuilder(_context).WithServiceConfig(Default().Build()).WithMotions(new Dictionary<int, string>
            {
                { 500, Detectors.toiletDetector },
                { 1500, Detectors.kitchenDetector },
                { 2000, Detectors.livingRoomDetector }
            }).Start();

            AdvanceToEnd();

            LampState(Detectors.toiletDetector).Should().BeTrue();
            LampState(Detectors.kitchenDetector).Should().BeTrue();
            LampState(Detectors.livingRoomDetector).Should().BeTrue();
        }

        [TestMethod]
        public async Task Move_Timeout_ShouldTurnOffAfterTimeout()
        {
            new LightAutomationEnviromentBuilder(_context).WithServiceConfig(Default().Build()).WithMotions(new Dictionary<int, string>
            {
                { 500, Detectors.kitchenDetector }
            }).Start();

            var area = await Query<AreaDescriptor>(AreaDescriptorQuery.Create(Detectors.toiletDetector));
            AdvanceJustAfterEnd();

            LampState(Detectors.kitchenDetector).Should().BeTrue();
            AdvanceTo(area.TurnOffTimeout);
            LampState(Detectors.kitchenDetector).Should().BeTrue();
            AdvanceJustAfter(area.TurnOffTimeout);
            LampState(Detectors.kitchenDetector).Should().BeFalse();
        }

        [TestMethod]
        public async Task Move_TurnOnAfterTurnOff_ShouldIncreaseTimeout()
        {
            new LightAutomationEnviromentBuilder(_context).WithServiceConfig(Default().Build()).WithMotions(new Dictionary<int, string>
            {
                { 500, Detectors.kitchenDetector },
                { 12000, Detectors.kitchenDetector },
            }).Start();

            AdvanceJustAfterEnd();
            var status = await Query<bool>(AutomationStateQuery.Create(Detectors.kitchenDetector));
            status.Should().BeTrue();
        }

        [TestMethod]
        public void Timeout_WhenPassthrough_ShouldBeQuick()
        {
            var timeout = TimeSpan.FromSeconds(5);
            var moveTime = TimeSpan.FromSeconds(9);
            var serviceConfig = Default();
            serviceConfig[Detectors.kitchenDetector].WithTimeout(timeout);

            new LightAutomationEnviromentBuilder(_context).WithServiceConfig(serviceConfig.Build()).WithRepeatedMotions(Detectors.kitchenDetector, moveTime).Start();

            AdvanceToEnd();
            LampState(Detectors.kitchenDetector).Should().BeTrue("After move we start counting and light should be on");
            AdvanceJustBefore(moveTime + timeout);
            LampState(Detectors.kitchenDetector).Should().BeTrue();
            AdvanceJustAfter(moveTime + timeout);
            LampState(Detectors.kitchenDetector).Should().BeFalse();
        }

        [TestMethod]
        public void Timeout_AsfterShortVisit_ShouldBeLonger()
        {
            var timeout = TimeSpan.FromSeconds(5);
            var moveTime = TimeSpan.FromSeconds(15);
            var serviceConfig = Default();
            serviceConfig[Detectors.kitchenDetector].WithTimeout(timeout);

            new LightAutomationEnviromentBuilder(_context).WithServiceConfig(serviceConfig.Build()).WithRepeatedMotions(Detectors.kitchenDetector, moveTime).Start();

            AdvanceToEnd();
            LampState(Detectors.kitchenDetector).Should().BeTrue("After move we start counting and light should be on");
            AdvanceJustBefore(moveTime + 2 * timeout);
            LampState(Detectors.kitchenDetector).Should().BeTrue();
            AdvanceJustAfter(moveTime + 2 * timeout);
            LampState(Detectors.kitchenDetector).Should().BeFalse();
        }

        [TestMethod]
        public void Timeout_AsfterLongVisit_ShouldBeLonger()
        {
            var timeout = TimeSpan.FromSeconds(5);
            var moveTime = TimeSpan.FromSeconds(75);
            var serviceConfig = Default();
            serviceConfig[Detectors.kitchenDetector].WithTimeout(timeout);

            new LightAutomationEnviromentBuilder(_context).WithServiceConfig(serviceConfig.Build()).WithRepeatedMotions(Detectors.kitchenDetector, moveTime).Start();

            AdvanceToEnd();
            LampState(Detectors.kitchenDetector).Should().BeTrue("After move we start counting and light should be on");
            AdvanceJustBefore(moveTime + 3 * timeout);
            LampState(Detectors.kitchenDetector).Should().BeTrue();
            AdvanceJustAfter(moveTime + 3 * timeout);
            LampState(Detectors.kitchenDetector).Should().BeFalse();
        }

        /// <summary>
        /// First not confused moving path should not be source of confusion for next path
        /// HT -> HL is eliminated because of that
        /// T -> HT -> K | L -> HL -> HT -> K
        /// </summary>
        [TestMethod]
        public async Task Confusion_NotConfusedVectors_ShouldNotBeConfused()
        {
            new LightAutomationEnviromentBuilder(_context).WithServiceConfig(Default().Build()).WithMotions(new Dictionary<int, string>
            {
                 // First path
                { 500, Detectors.toiletDetector },
                { 1500, Detectors.hallwayDetectorToilet },
                { 2000, Detectors.kitchenDetector },
                // Second path
                { 2500, Detectors.livingRoomDetector },
                { 3000, Detectors.hallwayDetectorLivingRoom }
            }).Start();

            AdvanceJustAfterEnd();

            var status = await Query<MotionStatus>(MotionServiceStatusQuery.Create());
            status.NumberOfConfusions.Should().Be(0);
            LampState(Detectors.kitchenDetector).Should().BeTrue();
        }

        [TestMethod]
        public void Confusion_MoveWithNoConfusion_ShouldTurnOfLightImmediately()
        {
            new LightAutomationEnviromentBuilder(_context).WithServiceConfig(Default().Build()).WithMotions(new Dictionary<int, string>
            {
                { 1000, Detectors.livingRoomDetector },
                { 2000, Detectors.hallwayDetectorLivingRoom },
                { 3000, Detectors.hallwayDetectorToilet },
                { 4000, Detectors.kitchenDetector },
            }).Start();

            AdvanceJustAfter(TimeSpan.FromSeconds(2));

            LampState(Detectors.livingRoomDetector).Should().BeTrue();
            LampState(Detectors.hallwayDetectorLivingRoom).Should().BeTrue();
            //  scheduler.AdvanceJustAfter(TimeSpan.FromSeconds(3));
            //   Assert.AreEqual(false, lampDictionary[Detectors.livingRoomDetector].IsTurnedOn);
            //Assert.AreEqual(true, lampDictionary[Detectors.hallwayDetectorLivingRoom].IsTurnedOn);
            //Assert.AreEqual(true, lampDictionary[Detectors.hallwayDetectorToilet].IsTurnedOn);
            //scheduler.AdvanceJustAfter(TimeSpan.FromSeconds(4));
            //Assert.AreEqual(false, lampDictionary[Detectors.livingRoomDetector].IsTurnedOn);
            //Assert.AreEqual(false, lampDictionary[Detectors.hallwayDetectorLivingRoom].IsTurnedOn);
            //Assert.AreEqual(true, lampDictionary[Detectors.hallwayDetectorToilet].IsTurnedOn);
            //Assert.AreEqual(true, lampDictionary[Detectors.kitchenDetector].IsTurnedOn);
            //scheduler.AdvanceJustAfter(TimeSpan.FromSeconds(5));
            //Assert.AreEqual(false, lampDictionary[Detectors.livingRoomDetector].IsTurnedOn);
            //Assert.AreEqual(false, lampDictionary[Detectors.hallwayDetectorLivingRoom].IsTurnedOn);
            //Assert.AreEqual(false, lampDictionary[Detectors.hallwayDetectorToilet].IsTurnedOn);
            //Assert.AreEqual(true, lampDictionary[Detectors.kitchenDetector].IsTurnedOn);
        }

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

        [TestMethod]
        public async Task Count_WhenCrossPassing_ShouldCountNumberOfPeople()
        {
            new LightAutomationEnviromentBuilder(_context).WithServiceConfig(Default().Build()).WithMotions(new Dictionary<int, string>
            {
                { 500, Detectors.kitchenDetector },
                { 501, Detectors.livingRoomDetector },

                { 1500, Detectors.hallwayDetectorToilet },
                { 1501, Detectors.hallwayDetectorLivingRoom },

                //OnNext(Time.Tics(2000), new MotionEnvelope(HallwaylivingRoomDetector)), <- Undetected due motion detectors lag after previous move
                //OnNext(Time.Tics(2000), new MotionEnvelope(HallwaytoiletDetector)),     <- Undetected due motion detectors lag after previous move

                { 3000, Detectors.livingRoomDetector },
                { 3001, Detectors.kitchenDetector }
            }).Start();

            AdvanceJustAfterEnd();

            var status = await Query<MotionStatus>(MotionServiceStatusQuery.Create());
            var kitchen = await Query<int>(NumberOfPeopleQuery.Create(Detectors.kitchenDetector));
            var living = await Query<int>(NumberOfPeopleQuery.Create(Detectors.livingRoomDetector));

            kitchen.Should().Be(1);
            living.Should().Be(1);
            //Assert.AreEqual(2, status.NumberOfPersonsInHouse); //TODO
        }

        [TestMethod]
        public async Task Count_TwoVectorsToSameRoom()
        {
            var servieConfig = Default().WithConfusionResolutionTime(TimeSpan.FromMilliseconds(5000)).Build();
            new LightAutomationEnviromentBuilder(_context).WithServiceConfig(servieConfig).WithMotions(new Dictionary<int, string>
            {
                { 500, Detectors.toiletDetector },
                { 1500, Detectors.hallwayDetectorToilet },
                { 2000, Detectors.kitchenDetector },
                { 2500, Detectors.livingRoomDetector },
                { 3000, Detectors.hallwayDetectorLivingRoom },
                { 3500, Detectors.hallwayDetectorToilet },
                { 4000, Detectors.kitchenDetector },
            }).Start();

            AdvanceTo(TimeSpan.FromMilliseconds(11000));

            var num = await Query<int>(NumberOfPeopleQuery.Create(Detectors.kitchenDetector));
            num.Should().Be(2);
        }

        [TestMethod]
        public async Task Test()
        {
            var servieConfig = Default().WithConfusionResolutionTime(TimeSpan.FromMilliseconds(5000)).Build();
            new LightAutomationEnviromentBuilder(_context).WithServiceConfig(servieConfig).WithMotions(new Dictionary<int, string>
            {
                //{ 500, Detectors.toiletDetector },
                { 501, Detectors.kitchenDetector },
                { 1000, Detectors.hallwayDetectorToilet },
             
            }).Start();

            AdvanceJustAfterEnd();

        }

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

        /// <summary>
        /// Get predefinied rooms configuration
        /// </summary>
        /// <returns></returns>
        private LightAutomationServiceBuilder Default()
        {
            var builder = new LightAutomationServiceBuilder();

            builder.WithRoom(new RoomBuilder(Rooms.Hallway).WithDetector(Detectors.hallwayDetectorToilet, new List<string> { Detectors.hallwayDetectorLivingRoom, Detectors.kitchenDetector, Detectors.staircaseDetector, Detectors.toiletDetector })
                                                           .WithDetector(Detectors.hallwayDetectorLivingRoom, new List<string> { Detectors.livingRoomDetector, Detectors.bathroomDetector, Detectors.hallwayDetectorToilet }));
            builder.WithRoom(new RoomBuilder(Rooms.Badroom).WithDetector(Detectors.badroomDetector, new List<string> { Detectors.hallwayDetectorLivingRoom }));
            builder.WithRoom(new RoomBuilder(Rooms.Balcony).WithDetector(Detectors.balconyDetector, new List<string> { Detectors.hallwayDetectorLivingRoom }));
            builder.WithRoom(new RoomBuilder(Rooms.Bathroom).WithDetector(Detectors.bathroomDetector, new List<string> { Detectors.hallwayDetectorLivingRoom }));
            builder.WithRoom(new RoomBuilder(Rooms.Kitchen).WithDetector(Detectors.kitchenDetector, new List<string> { Detectors.hallwayDetectorToilet }));
            builder.WithRoom(new RoomBuilder(Rooms.Livingroom).WithDetector(Detectors.livingRoomDetector, new List<string> { Detectors.livingRoomDetector }));
            builder.WithRoom(new RoomBuilder(Rooms.Staircase).WithDetector(Detectors.staircaseDetector, new List<string> { Detectors.hallwayDetectorToilet }));
            builder.WithRoom(new RoomBuilder(Rooms.Toilet).WithDetector(Detectors.toiletDetector, new List<string> { Detectors.hallwayDetectorToilet }).WithProperty(MotionProperties.MaxPersonCapacity, "1"));

            return builder;
        }
    }
}