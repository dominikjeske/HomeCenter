using HomeCenter.Model.Core;
using HomeCenter.Services.MotionService.Commands;
using HomeCenter.Services.MotionService.Model;
using Microsoft.Reactive.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
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
        public void MoveInRoomShouldTurnOnLight()
        {
            var (motionEvents, scheduler, lampDictionary) = new LightAutomationServiceBuilder(_context).WithMotion
            (
              OnNext(Time.Tics(500), new MotionEnvelope(Detectors.toiletDetector)),
              OnNext(Time.Tics(1500), new MotionEnvelope(Detectors.kitchenDetector)),
              OnNext(Time.Tics(2000), new MotionEnvelope(Detectors.livingRoomDetector))
            ).Build();

            scheduler.AdvanceToEnd(motionEvents);

            Assert.AreEqual(true, lampDictionary[Detectors.toiletDetector].IsTurnedOn);
            Assert.AreEqual(true, lampDictionary[Detectors.kitchenDetector].IsTurnedOn);
            Assert.AreEqual(true, lampDictionary[Detectors.livingRoomDetector].IsTurnedOn);
        }

        [TestMethod]
        public void MoveInRoomShouldTurnOnLightOnWhenWorkinghoursAreDaylight()
        {
            var (motionEvents, scheduler, lampDictionary) = new LightAutomationServiceBuilder(_context).WithWorkingTime(WorkingTime.DayLight).WithMotion
            (
              OnNext(Time.Tics(500), new MotionEnvelope(Detectors.toiletDetector)),
              OnNext(Time.Tics(1500), new MotionEnvelope(Detectors.kitchenDetector)),
              OnNext(Time.Tics(2000), new MotionEnvelope(Detectors.livingRoomDetector))
            ).Build();

            SystemTime.Set(TimeSpan.FromHours(12));

            scheduler.AdvanceToEnd(motionEvents);

            Assert.AreEqual(true, lampDictionary[Detectors.toiletDetector].IsTurnedOn);
            Assert.AreEqual(true, lampDictionary[Detectors.kitchenDetector].IsTurnedOn);
            Assert.AreEqual(true, lampDictionary[Detectors.livingRoomDetector].IsTurnedOn);
        }

        [TestMethod]
        public void MoveInRoomShouldNotTurnOnLightOnNightWhenWorkinghoursAreDaylight()
        {
            var (motionEvents, scheduler, lampDictionary) = new LightAutomationServiceBuilder(_context).WithWorkingTime(WorkingTime.DayLight).WithMotion
            (
              OnNext(Time.Tics(500), new MotionEnvelope(Detectors.toiletDetector)),
              OnNext(Time.Tics(1500), new MotionEnvelope(Detectors.kitchenDetector)),
              OnNext(Time.Tics(2000), new MotionEnvelope(Detectors.livingRoomDetector))
            ).Build();

            SystemTime.Set(TimeSpan.FromHours(21));

            scheduler.AdvanceToEnd(motionEvents);

            Assert.AreEqual(false, lampDictionary[Detectors.toiletDetector].IsTurnedOn);
            Assert.AreEqual(false, lampDictionary[Detectors.kitchenDetector].IsTurnedOn);
            Assert.AreEqual(false, lampDictionary[Detectors.livingRoomDetector].IsTurnedOn);
        }

        [TestMethod]
        public void MoveInRoomShouldNotTurnOnLightOnDaylightWhenWorkinghoursIsNight()
        {
            var (motionEvents, scheduler, lampDictionary) = new LightAutomationServiceBuilder(_context).WithWorkingTime(WorkingTime.AfterDusk).WithMotion
            (
              OnNext(Time.Tics(500), new MotionEnvelope(Detectors.toiletDetector)),
              OnNext(Time.Tics(1500), new MotionEnvelope(Detectors.kitchenDetector)),
              OnNext(Time.Tics(2000), new MotionEnvelope(Detectors.livingRoomDetector))
            ).Build();

            SystemTime.Set(TimeSpan.FromHours(12));

            scheduler.AdvanceToEnd(motionEvents);

            Assert.AreEqual(false, lampDictionary[Detectors.toiletDetector].IsTurnedOn);
            Assert.AreEqual(false, lampDictionary[Detectors.kitchenDetector].IsTurnedOn);
            Assert.AreEqual(false, lampDictionary[Detectors.livingRoomDetector].IsTurnedOn);
        }

        [TestMethod]
        public async Task MoveInRoomShouldNotTurnOnLightWhenAutomationIsDisabled()
        {
            var (motionEvents, scheduler, lampDictionary) = new LightAutomationServiceBuilder(_context).WithMotion
            (
              OnNext(Time.Tics(500), new MotionEnvelope(Detectors.toiletDetector))
            ).Build();

            _context.Send(DisableAutomationCommand.Create(Detectors.toiletDetector));
            scheduler.AdvanceToEnd(motionEvents);

            Assert.AreEqual(false, lampDictionary[Detectors.toiletDetector].IsTurnedOn);
            Assert.AreEqual(true, await _context.Query<bool>(AutomationStateQuery.Create(Detectors.toiletDetector)));
        }

        [TestMethod]
        public void MoveInRoomShouldTurnOnLightWhenAutomationIsReEnabled()
        {
            var (motionEvents, scheduler, lampDictionary) = new LightAutomationServiceBuilder(_context).WithMotion
            (
              OnNext(Time.Tics(500), new MotionEnvelope(Detectors.toiletDetector)),
              OnNext(Time.Tics(2500), new MotionEnvelope(Detectors.toiletDetector))
            ).Build();

            _context.Send(DisableAutomationCommand.Create(Detectors.toiletDetector));
            motionEvents.Subscribe(_ => _context.Send(EnableAutomationCommand.Create(Detectors.toiletDetector)));
            scheduler.AdvanceToEnd(motionEvents);

            Assert.AreEqual(true, lampDictionary[Detectors.toiletDetector].IsTurnedOn);
        }

        [TestMethod]
        public async Task AnalyzeMoveShouldCountPeopleNumberInRoom()
        {
            var (motionEvents, scheduler, lampDictionary) = new LightAutomationServiceBuilder(_context).WithConfusionResolutionTime(TimeSpan.FromMilliseconds(5000))
                                                                                                       .WithMotion
            (
              OnNext(Time.Tics(500), new MotionEnvelope(Detectors.toiletDetector)),
              OnNext(Time.Tics(1500), new MotionEnvelope(Detectors.hallwayDetectorToilet)),
              OnNext(Time.Tics(2000), new MotionEnvelope(Detectors.kitchenDetector)),
              OnNext(Time.Tics(2500), new MotionEnvelope(Detectors.livingRoomDetector)),
              OnNext(Time.Tics(3000), new MotionEnvelope(Detectors.hallwayDetectorLivingRoom)),
              OnNext(Time.Tics(3500), new MotionEnvelope(Detectors.hallwayDetectorToilet)),
              OnNext(Time.Tics(4000), new MotionEnvelope(Detectors.kitchenDetector))
            ).Build();

            scheduler.AdvanceTo(TimeSpan.FromMilliseconds(11000));

            var num = await _context.Query<int>(NumberOfPeopleQuery.Create(Detectors.kitchenDetector));
            Assert.AreEqual(2, num);
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
        public void WhenLeaveFromOnePersonRoomWithNoConfusionShouldTurnOffLightImmediately()
        {
            var (motionEvents, scheduler, lampDictionary) = new LightAutomationServiceBuilder(_context).WithMotion
            (
              OnNext(Time.Tics(500), new MotionEnvelope(Detectors.toiletDetector)),
              OnNext(Time.Tics(1500), new MotionEnvelope(Detectors.hallwayDetectorToilet))
            ).Build();

            scheduler.AdvanceJustAfterEnd(motionEvents);

            Assert.AreEqual(false, lampDictionary[Detectors.toiletDetector].IsTurnedOn);
        }

        [TestMethod]
        public void WhenLeaveFromOnePersonRoomWithConfusionShouldTurnOffWhenConfusionResolved()
        {
            var confusionResolutionTime = TimeSpan.FromMilliseconds(5000);
            var (motionEvents, scheduler, lampDictionary) = new LightAutomationServiceBuilder(_context).WithMotion
            (
                  OnNext(Time.Tics(500), new MotionEnvelope(Detectors.toiletDetector)),
                  OnNext(Time.Tics(1000), new MotionEnvelope(Detectors.kitchenDetector)),
                  OnNext(Time.Tics(1500), new MotionEnvelope(Detectors.hallwayDetectorToilet)),
                  OnNext(Time.Tics(2000), new MotionEnvelope(Detectors.hallwayDetectorLivingRoom)),
                  OnNext(Time.Tics(3000), new MotionEnvelope(Detectors.kitchenDetector))
            ).Build();

            scheduler.AdvanceTo(confusionResolutionTime + TimeSpan.FromMilliseconds(1500));

            Assert.AreEqual(false, lampDictionary[Detectors.toiletDetector].IsTurnedOn);
        }

        [TestMethod]
        public void WhenLeaveFromRoomWithNoConfusionShouldTurnOffLightAfterSomeTime()
        {
            var (motionEvents, scheduler, lampDictionary) = new LightAutomationServiceBuilder(_context).WithMotion
            (
              OnNext(Time.Tics(500), new MotionEnvelope(Detectors.kitchenDetector)),
              OnNext(Time.Tics(1500), new MotionEnvelope(Detectors.hallwayDetectorToilet))
            ).Build();

            scheduler.AdvanceJustAfterEnd(motionEvents);

            Assert.AreEqual(true, lampDictionary[Detectors.kitchenDetector].IsTurnedOn);
            scheduler.AdvanceTo(Time.Tics(2500));
            Assert.AreEqual(false, lampDictionary[Detectors.kitchenDetector].IsTurnedOn);
        }

        [TestMethod]
        public async Task WhenNoMoveInRoomShouldTurnOffAfterTurnOffTimeout()
        {
            var (motionEvents, scheduler, lampDictionary) = new LightAutomationServiceBuilder(_context).WithMotion
            (
                OnNext(Time.Tics(500), new MotionEnvelope(Detectors.kitchenDetector))
            ).Build();

            var area = await _context.Query<AreaDescriptor>(AreaDescriptorQuery.Create(Detectors.toiletDetector));
            scheduler.AdvanceJustAfterEnd(motionEvents);

            Assert.AreEqual(true, lampDictionary[Detectors.kitchenDetector].IsTurnedOn);
            scheduler.AdvanceTo(area.TurnOffTimeout);
            Assert.AreEqual(true, lampDictionary[Detectors.kitchenDetector].IsTurnedOn);
            scheduler.AdvanceJustAfter(area.TurnOffTimeout);
            Assert.AreEqual(false, lampDictionary[Detectors.kitchenDetector].IsTurnedOn);
        }

        /// <summary>
        /// First not confused moving path should not be source of confusion for next path
        /// HT -> HL is eliminated because of that
        /// T -> HT -> K | L -> HL -> HT -> K
        /// </summary>
        [TestMethod]
        public async Task MoveInFirstPathShouldNotConfusedNextPathWhenItIsSure()
        {
            var (motionEvents, scheduler, lampDictionary) = new LightAutomationServiceBuilder(_context).WithMotion
            (
                // First path
                OnNext(Time.Tics(500), new MotionEnvelope(Detectors.toiletDetector)),
                OnNext(Time.Tics(1500), new MotionEnvelope(Detectors.hallwayDetectorToilet)),
                OnNext(Time.Tics(2000), new MotionEnvelope(Detectors.kitchenDetector)),
                // Second path
                OnNext(Time.Tics(2500), new MotionEnvelope(Detectors.livingRoomDetector)),
                OnNext(Time.Tics(3000), new MotionEnvelope(Detectors.hallwayDetectorLivingRoom))
            ).Build();

            scheduler.AdvanceJustAfterEnd(motionEvents);

            var status = await _context.Query<MotionStatus>(MotionServiceStatusQuery.Create());
            Assert.AreEqual(0, status.NumberOfConfusions);
            Assert.AreEqual(true, lampDictionary[Detectors.kitchenDetector].IsTurnedOn);
        }

        [TestMethod]
        public async Task WhenCrossPassingNumberOfPeopleSlouldBeCorrect()
        {
            var (motionEvents, scheduler, lampDictionary) = new LightAutomationServiceBuilder(_context).WithMotion
            (
                OnNext(Time.Tics(500), new MotionEnvelope(Detectors.kitchenDetector)),
                OnNext(Time.Tics(501), new MotionEnvelope(Detectors.livingRoomDetector)),

                OnNext(Time.Tics(1500), new MotionEnvelope(Detectors.hallwayDetectorToilet)),
                OnNext(Time.Tics(1501), new MotionEnvelope(Detectors.hallwayDetectorLivingRoom)),

                //OnNext(Time.Tics(2000), new MotionEnvelope(HallwaylivingRoomDetector)), <- Undetected due motion detectors lag after previous move
                //OnNext(Time.Tics(2000), new MotionEnvelope(HallwaytoiletDetector)),     <- Undetected due motion detectors lag after previous move

                OnNext(Time.Tics(3000), new MotionEnvelope(Detectors.livingRoomDetector)),
                OnNext(Time.Tics(3001), new MotionEnvelope(Detectors.kitchenDetector))
            ).Build();

            scheduler.AdvanceJustAfterEnd(motionEvents);

            var status = await _context.Query<MotionStatus>(MotionServiceStatusQuery.Create());
            var kitchen = await _context.Query<int>(NumberOfPeopleQuery.Create(Detectors.kitchenDetector));
            var living = await _context.Query<int>(NumberOfPeopleQuery.Create(Detectors.livingRoomDetector));
            Assert.AreEqual(1, kitchen);
            Assert.AreEqual(1, living);
            //Assert.AreEqual(2, status.NumberOfPersonsInHouse); //TODO
        }

       
        [TestMethod]
        public async Task WhenTurnOnJustAfterTurnOffServiceShouldIncreaseTurnOffTimeout()
        {
            var (motionEvents, scheduler, lampDictionary) = new LightAutomationServiceBuilder(_context).WithMotion
            (
                OnNext(Time.Tics(500), new MotionEnvelope(Detectors.kitchenDetector)),
                OnNext(Time.Tics(12000), new MotionEnvelope(Detectors.kitchenDetector))
            ).Build();

            scheduler.AdvanceJustAfterEnd(motionEvents);

            Assert.AreEqual(false, await _context.Query<bool>(AutomationStateQuery.Create(Detectors.kitchenDetector)));
        }

        [TestMethod]
        public void WhenNoConfusionMoveThroughManyRoomsShouldTurnOfLightImmediately()
        {
            var (motionEvents, scheduler, lampDictionary) = new LightAutomationServiceBuilder(_context).WithMotion
            (
                OnNext(Time.Tics(1000), new MotionEnvelope(Detectors.livingRoomDetector)),
                OnNext(Time.Tics(2000), new MotionEnvelope(Detectors.hallwayDetectorLivingRoom)),
                OnNext(Time.Tics(3000), new MotionEnvelope(Detectors.hallwayDetectorToilet)),
                OnNext(Time.Tics(4000), new MotionEnvelope(Detectors.kitchenDetector))
            ).Build();

            scheduler.AdvanceJustAfter(TimeSpan.FromSeconds(2));

            Assert.AreEqual(true, lampDictionary[Detectors.livingRoomDetector].IsTurnedOn);
            Assert.AreEqual(true, lampDictionary[Detectors.hallwayDetectorLivingRoom].IsTurnedOn);
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
        public async Task WhenTwoPersonsStartsFromOneRoomAndSplitToTwoOthersNumbersOfPeopleShouldBeCorrect()
        {
            var (motionEvents, scheduler, lampDictionary) = new LightAutomationServiceBuilder(_context).WithMotion
            (
                OnNext(Time.Tics(1000), new MotionEnvelope(Detectors.livingRoomDetector)),
                OnNext(Time.Tics(2000), new MotionEnvelope(Detectors.hallwayDetectorLivingRoom)),
                OnNext(Time.Tics(2900), new MotionEnvelope(Detectors.bathroomDetector)),
                OnNext(Time.Tics(3000), new MotionEnvelope(Detectors.hallwayDetectorToilet)),
                OnNext(Time.Tics(4000), new MotionEnvelope(Detectors.kitchenDetector))
            ).Build();

            scheduler.AdvanceJustAfter(TimeSpan.FromSeconds(4));

            var status = await _context.Query<MotionStatus>(MotionServiceStatusQuery.Create());
            Assert.AreEqual(1, await _context.Query<int>(NumberOfPeopleQuery.Create(Detectors.kitchenDetector)));
            Assert.AreEqual(1, await _context.Query<int>(NumberOfPeopleQuery.Create(Detectors.bathroomDetector)));
          //  Assert.AreEqual(2, status.NumberOfPersonsInHouse);
            Assert.AreEqual(true, lampDictionary[Detectors.kitchenDetector].IsTurnedOn);
            Assert.AreEqual(true, lampDictionary[Detectors.bathroomDetector].IsTurnedOn);
            Assert.AreEqual(false, lampDictionary[Detectors.hallwayDetectorLivingRoom].IsTurnedOn);
            //Assert.AreEqual(false, lampDictionary[Detectors.livingRoomDetector].IsTurnedOn);
            scheduler.AdvanceJustAfter(TimeSpan.FromSeconds(5));
            Assert.AreEqual(false, lampDictionary[Detectors.hallwayDetectorToilet].IsTurnedOn);
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
    }
}