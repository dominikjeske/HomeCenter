using FluentAssertions;
using HomeCenter.Model.Core;
using HomeCenter.Model.Messages.Commands;
using HomeCenter.Model.Messages.Queries;
using HomeCenter.Services.MotionService.Commands;
using HomeCenter.Services.MotionService.Model;
using Microsoft.Reactive.Testing;
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
    public class LightAutomationServiceTests : ReactiveTest
    {
        private ActorContext _context;

        [TestInitialize]
        public void PrepareContext()
        {
            _context = new ActorContext();
        }

        [TestCleanup]
        public Task CleanContext()
        {
            return _context.PID.StopAsync();
        }

        [TestMethod]
        public void Move_OnSeparateRooms_ShouldTurnOnLight()
        {
            new LightAutomationEnviromentBuilder(_context).WithServiceConfig(DefaultServiceConfig().Build()).WithMotions(new Dictionary<int, string>
            {
                { 500, Detectors.toiletDetector },
                { 1500, Detectors.kitchenDetector },
                { 2000, Detectors.livingRoomDetector }
            }).Start();

            AdvanceToEnd();

            IsTurnedOn(Detectors.toiletDetector).Should().BeTrue();
            IsTurnedOn(Detectors.kitchenDetector).Should().BeTrue();
            IsTurnedOn(Detectors.livingRoomDetector).Should().BeTrue();
        }

        [TestMethod]
        public void Move_DuringDaylight_ShuldPowerOnDayLights()
        {
            var servieConfig = DefaultServiceConfig().WithWorkingTime(WorkingTime.DayLight).Build();
            new LightAutomationEnviromentBuilder(_context).WithServiceConfig(servieConfig).WithMotions(new Dictionary<int, string>
            {
                { 500, Detectors.toiletDetector },
                { 1500, Detectors.kitchenDetector },
                { 2000, Detectors.livingRoomDetector }
            }).Start();

            SystemTime.Set(TimeSpan.FromHours(12));

            AdvanceToEnd();

            IsTurnedOn(Detectors.toiletDetector).Should().BeTrue();
            IsTurnedOn(Detectors.kitchenDetector).Should().BeTrue();
            IsTurnedOn(Detectors.livingRoomDetector).Should().BeTrue();
        }

        [TestMethod]
        public void Move_AfterDusk_ShouldNotPowerOnDayLight()
        {
            var servieConfig = DefaultServiceConfig().WithWorkingTime(WorkingTime.DayLight).Build();
            new LightAutomationEnviromentBuilder(_context).WithServiceConfig(servieConfig).WithMotions(new Dictionary<int, string>
            {
                { 500, Detectors.toiletDetector },
                { 1500, Detectors.kitchenDetector },
                { 2000, Detectors.livingRoomDetector }
            }).Start();

            SystemTime.Set(TimeSpan.FromHours(21));

            AdvanceToEnd();

            IsTurnedOn(Detectors.toiletDetector).Should().BeFalse();
            IsTurnedOn(Detectors.kitchenDetector).Should().BeFalse();
            IsTurnedOn(Detectors.livingRoomDetector).Should().BeFalse();
        }

        [TestMethod]
        public void Move_DuringDaylight_ShuldNotPowerOnNightLight()
        {
            var servieConfig = DefaultServiceConfig().WithWorkingTime(WorkingTime.AfterDusk).Build();
            new LightAutomationEnviromentBuilder(_context).WithServiceConfig(servieConfig).WithMotions(new Dictionary<int, string>
            {
                { 500, Detectors.toiletDetector },
                { 1500, Detectors.kitchenDetector },
                { 2000, Detectors.livingRoomDetector }
            }).Start();

            SystemTime.Set(TimeSpan.FromHours(12));

            AdvanceToEnd();

            IsTurnedOn(Detectors.toiletDetector).Should().BeFalse();
            IsTurnedOn(Detectors.kitchenDetector).Should().BeFalse();
            IsTurnedOn(Detectors.livingRoomDetector).Should().BeFalse();
        }

        [TestMethod]
        public async Task Move_WhenAutomationDisabled_ShouldNoturnOnLights()
        {
            new LightAutomationEnviromentBuilder(_context).WithServiceConfig(DefaultServiceConfig().Build()).WithMotions(new Dictionary<int, string>
            {
                { 500, Detectors.toiletDetector }
            }).Start();

            SendCommand(DisableAutomationCommand.Create(Detectors.toiletDetector));
            AdvanceToEnd();
            
            IsTurnedOn(Detectors.toiletDetector).Should().BeFalse();
            var automationState = await Query<bool>(AutomationStateQuery.Create(Detectors.toiletDetector));
            automationState.Should().BeTrue(); //??
        }

        [TestMethod]
        public void Move_WhenReEnableAutomation_ShouldTurnOnLights()
        {
            new LightAutomationEnviromentBuilder(_context).WithServiceConfig(DefaultServiceConfig().Build()).WithMotions(new Dictionary<int, string>
            {
                { 500, Detectors.toiletDetector },
                { 2500, Detectors.toiletDetector }
            }).Start();

            SendCommand(DisableAutomationCommand.Create(Detectors.toiletDetector));
            RunAfterFirstMove(_ => SendCommand(EnableAutomationCommand.Create(Detectors.toiletDetector)));
            AdvanceToEnd();

            IsTurnedOn(Detectors.toiletDetector).Should().BeTrue();
        }

        [TestMethod]
        public async Task Move_InRoom_ShouldCountPeople()
        {
            var servieConfig = DefaultServiceConfig().WithConfusionResolutionTime(TimeSpan.FromMilliseconds(5000)).Build();
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

        [TestMethod]
        public void Leave_FromOnePeopleRoomWithNoConfusion_ShouldTurnOffLightImmediately()
        {
            new LightAutomationEnviromentBuilder(_context).WithServiceConfig(DefaultServiceConfig().Build()).WithMotions(new Dictionary<int, string>
            {
                { 500, Detectors.toiletDetector },
                { 1500, Detectors.hallwayDetectorToilet }
            }).Start();

            AdvanceJustAfterEnd();

            IsTurnedOn(Detectors.toiletDetector).Should().BeFalse();
        }

        [TestMethod]
        public void Leave_FromOnePeopleRoomWithConfusion_ShouldTurnOffAfterResolvingConfusion()
        {
            var confusionResolutionTime = TimeSpan.FromMilliseconds(5000);
            var servieConfig = DefaultServiceConfig().WithConfusionResolutionTime(confusionResolutionTime).Build();
            new LightAutomationEnviromentBuilder(_context).WithServiceConfig(servieConfig).WithMotions(new Dictionary<int, string>
            {
                { 500, Detectors.toiletDetector },
                { 1000, Detectors.kitchenDetector },
                { 1500, Detectors.hallwayDetectorToilet },
                { 2000, Detectors.hallwayDetectorLivingRoom },
                { 3000, Detectors.kitchenDetector },

            }).Start();
                        
            AdvanceTo(confusionResolutionTime + TimeSpan.FromMilliseconds(1500));

            IsTurnedOn(Detectors.toiletDetector).Should().BeFalse();
        }

        [TestMethod]
        public void Leave_FromRoomWithConfusion_ShouldTurnOffLightAfterConfusionResolved()
        {
            new LightAutomationEnviromentBuilder(_context).WithServiceConfig(DefaultServiceConfig().Build()).WithMotions(new Dictionary<int, string>
            {
                { 500, Detectors.kitchenDetector },
                { 1500, Detectors.hallwayDetectorToilet }
            }).Start();

            AdvanceJustAfterEnd();

            IsTurnedOn(Detectors.kitchenDetector).Should().BeTrue();
            AdvanceTo(Time.Tics(2500));
            IsTurnedOn(Detectors.kitchenDetector).Should().BeFalse();
        }

        [TestMethod]
        public async Task NoMove_InRoom_ShouldTurnOffAfterTimeout()
        {
            new LightAutomationEnviromentBuilder(_context).WithServiceConfig(DefaultServiceConfig().Build()).WithMotions(new Dictionary<int, string>
            {
                { 500, Detectors.kitchenDetector }
            }).Start();

            var area = await Query<AreaDescriptor>(AreaDescriptorQuery.Create(Detectors.toiletDetector));
            AdvanceJustAfterEnd();

            IsTurnedOn(Detectors.kitchenDetector).Should().BeTrue();
            AdvanceTo(area.TurnOffTimeout);
            IsTurnedOn(Detectors.kitchenDetector).Should().BeTrue();
            AdvanceJustAfter(area.TurnOffTimeout);
            IsTurnedOn(Detectors.kitchenDetector).Should().BeFalse();
        }

        /// <summary>
        /// First not confused moving path should not be source of confusion for next path
        /// HT -> HL is eliminated because of that
        /// T -> HT -> K | L -> HL -> HT -> K
        /// </summary>
        [TestMethod]
        public async Task Move_NotConfusedVectors_ShouldNotBeConfused()
        {
            new LightAutomationEnviromentBuilder(_context).WithServiceConfig(DefaultServiceConfig().Build()).WithMotions(new Dictionary<int, string>
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
            IsTurnedOn(Detectors.kitchenDetector).Should().BeTrue();
        }

        [TestMethod]
        public async Task Move_WhenCrossPassing_ShouldCountNumberOfPeople()
        {
            new LightAutomationEnviromentBuilder(_context).WithServiceConfig(DefaultServiceConfig().Build()).WithMotions(new Dictionary<int, string>
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
        public async Task TurnOn_AfterTurnOff_ShouldIncreaseTimeout()
        {
            new LightAutomationEnviromentBuilder(_context).WithServiceConfig(DefaultServiceConfig().Build()).WithMotions(new Dictionary<int, string>
            {
                { 500, Detectors.kitchenDetector },
                { 12000, Detectors.kitchenDetector },
            }).Start();

            AdvanceJustAfterEnd();
            var status = await Query<bool>(AutomationStateQuery.Create(Detectors.kitchenDetector));
            status.Should().BeFalse();
        }

        [TestMethod]
        public void Move_ThroughManyWithNoConfusion_ShouldTurnOfLightImmediately()
        {
            new LightAutomationEnviromentBuilder(_context).WithServiceConfig(DefaultServiceConfig().Build()).WithMotions(new Dictionary<int, string>
            {
                { 1000, Detectors.livingRoomDetector },
                { 2000, Detectors.hallwayDetectorLivingRoom },
                { 3000, Detectors.hallwayDetectorToilet },
                { 4000, Detectors.kitchenDetector },
            }).Start();

            AdvanceJustAfter(TimeSpan.FromSeconds(2));

            IsTurnedOn(Detectors.livingRoomDetector).Should().BeTrue();
            IsTurnedOn(Detectors.hallwayDetectorLivingRoom).Should().BeTrue();
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

        [TestMethod]
        public async Task Move_SplitFromOneRoom_ShouldCountPeople()
        {
            new LightAutomationEnviromentBuilder(_context).WithServiceConfig(DefaultServiceConfig().Build()).WithMotions(new Dictionary<int, string>
            {
                { 1000, Detectors.livingRoomDetector },
                { 2000, Detectors.hallwayDetectorLivingRoom },
                { 2900, Detectors.bathroomDetector },
                { 3000, Detectors.hallwayDetectorToilet },
                { 4000, Detectors.kitchenDetector },
            }).Start();

            AdvanceJustAfter(TimeSpan.FromSeconds(4));

            var status = await Query<MotionStatus>(MotionServiceStatusQuery.Create());
            var numKitchen = await Query<int>(NumberOfPeopleQuery.Create(Detectors.kitchenDetector));
            var numBathroom = await Query<int>(NumberOfPeopleQuery.Create(Detectors.bathroomDetector));

            numKitchen.Should().Be(1);
            numBathroom.Should().Be(1);

            IsTurnedOn(Detectors.kitchenDetector).Should().BeTrue();
            IsTurnedOn(Detectors.bathroomDetector).Should().BeTrue();
            IsTurnedOn(Detectors.hallwayDetectorLivingRoom).Should().BeFalse();
            //  Assert.AreEqual(2, status.NumberOfPersonsInHouse);
            //Assert.AreEqual(false, lampDictionary[Detectors.livingRoomDetector].IsTurnedOn);
            AdvanceJustAfter(TimeSpan.FromSeconds(5));
            IsTurnedOn(Detectors.hallwayDetectorToilet).Should().BeFalse();
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


        private LightAutomationServiceBuilder DefaultServiceConfig()
        {
            var builder = new LightAutomationServiceBuilder();

            builder.WithArea(Areas.Hallway);
            builder.WithArea(Areas.Badroom);
            builder.WithArea(Areas.Balcony);
            builder.WithArea(Areas.Bathroom);
            builder.WithArea(Areas.Kitchen);
            builder.WithArea(Areas.Livingroom);
            builder.WithArea(Areas.Staircase);
            builder.WithArea(Areas.Toilet).WithAreaProperty(Areas.Toilet, MotionProperties.MaxPersonCapacity, "1");

            builder.WithDetector(Detectors.hallwayDetectorToilet, Areas.Hallway, new List<string> { Detectors.hallwayDetectorLivingRoom, Detectors.kitchenDetector, Detectors.staircaseDetector, Detectors.toiletDetector });
            builder.WithDetector(Detectors.hallwayDetectorLivingRoom, Areas.Hallway, new List<string> { Detectors.livingRoomDetector, Detectors.bathroomDetector, Detectors.hallwayDetectorToilet });
            builder.WithDetector(Detectors.livingRoomDetector, Areas.Livingroom, new List<string> { Detectors.livingRoomDetector });
            builder.WithDetector(Detectors.balconyDetector, Areas.Balcony, new List<string> { Detectors.hallwayDetectorLivingRoom });
            builder.WithDetector(Detectors.kitchenDetector, Areas.Kitchen, new List<string> { Detectors.hallwayDetectorToilet });
            builder.WithDetector(Detectors.bathroomDetector, Areas.Bathroom, new List<string> { Detectors.hallwayDetectorLivingRoom });
            builder.WithDetector(Detectors.badroomDetector, Areas.Badroom, new List<string> { Detectors.hallwayDetectorLivingRoom });
            builder.WithDetector(Detectors.staircaseDetector, Areas.Staircase, new List<string> { Detectors.hallwayDetectorToilet });
            builder.WithDetector(Detectors.toiletDetector, Areas.Toilet, new List<string> { Detectors.hallwayDetectorToilet });

            return builder;
        }

        private void AdvanceToEnd() => _context.Scheduler.AdvanceToEnd(_context.MotionEvents);

        private void AdvanceTo(TimeSpan time) => _context.Scheduler.AdvanceTo(time);

        private void AdvanceTo(long ticks) => _context.Scheduler.AdvanceTo(ticks);

        private void AdvanceJustAfterEnd() => _context.Scheduler.AdvanceJustAfterEnd(_context.MotionEvents);

        private void AdvanceJustAfter(TimeSpan time) => _context.Scheduler.AdvanceJustAfter(time);

        private bool IsTurnedOn(string lamp) => _context.Lamps[lamp].IsTurnedOn;

        private void SendCommand(Command command) => _context.Send(command);

        private Task<T> Query<T>(Query query) => _context.Query<T>(query);

        private void RunAfterFirstMove(Action<MotionEnvelope> action) => _context.MotionEvents.Subscribe(action);
    }
}