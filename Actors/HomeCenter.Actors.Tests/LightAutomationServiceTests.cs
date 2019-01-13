using HomeCenter.Model.Core;
using HomeCenter.Model.Messages.Events.Device;
using HomeCenter.Services.MotionService.Model;
using Microsoft.Reactive.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

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
        [TestMethod]
        public void MoveInRoomShouldTurnOnLight()
        {
            var (service, motionEvents, scheduler, lampDictionary) = new LightAutomationServiceBuilder().WithMotion
            (
              OnNext(Time.Tics(500), new MotionEnvelope(Detectors.toiletDetector)),
              OnNext(Time.Tics(1500), new MotionEnvelope(Detectors.kitchenDetector)),
              OnNext(Time.Tics(2000), new MotionEnvelope(Detectors.livingRoomDetector))
            ).Build();

            service.Start();
            scheduler.AdvanceToEnd(motionEvents);

            Assert.AreEqual(true, lampDictionary[Detectors.toiletDetector].IsTurnedOn);
            Assert.AreEqual(true, lampDictionary[Detectors.kitchenDetector].IsTurnedOn);
            Assert.AreEqual(true, lampDictionary[Detectors.livingRoomDetector].IsTurnedOn);
        }

        [TestMethod]
        public void MoveInRoomShouldTurnOnLightOnWhenWorkinghoursAreDaylight()
        {
            var (service, motionEvents, scheduler, lampDictionary) = new LightAutomationServiceBuilder().WithAreaDescriptor(new AreaDescriptor { WorkingTime = WorkingTime.DayLight }).WithMotion
            (
              OnNext(Time.Tics(500), new MotionEnvelope(Detectors.toiletDetector)),
              OnNext(Time.Tics(1500), new MotionEnvelope(Detectors.kitchenDetector)),
              OnNext(Time.Tics(2000), new MotionEnvelope(Detectors.livingRoomDetector))
            ).Build();

            SystemTime.Set(TimeSpan.FromHours(12));

            service.Start();
            scheduler.AdvanceToEnd(motionEvents);

            Assert.AreEqual(true, lampDictionary[Detectors.toiletDetector].IsTurnedOn);
            Assert.AreEqual(true, lampDictionary[Detectors.kitchenDetector].IsTurnedOn);
            Assert.AreEqual(true, lampDictionary[Detectors.livingRoomDetector].IsTurnedOn);
        }

        [TestMethod]
        public void MoveInRoomShouldNotTurnOnLightOnNightWhenWorkinghoursAreDaylight()
        {
            var (service, motionEvents, scheduler, lampDictionary) = new LightAutomationServiceBuilder().WithAreaDescriptor(new AreaDescriptor { WorkingTime = WorkingTime.DayLight }).WithMotion
            (
              OnNext(Time.Tics(500), new MotionEnvelope(Detectors.toiletDetector)),
              OnNext(Time.Tics(1500), new MotionEnvelope(Detectors.kitchenDetector)),
              OnNext(Time.Tics(2000), new MotionEnvelope(Detectors.livingRoomDetector))
            ).Build();

            SystemTime.Set(TimeSpan.FromHours(21));

            service.Start();
            scheduler.AdvanceToEnd(motionEvents);

            Assert.AreEqual(false, lampDictionary[Detectors.toiletDetector].IsTurnedOn);
            Assert.AreEqual(false, lampDictionary[Detectors.kitchenDetector].IsTurnedOn);
            Assert.AreEqual(false, lampDictionary[Detectors.livingRoomDetector].IsTurnedOn);
        }

        [TestMethod]
        public void MoveInRoomShouldNotTurnOnLightOnDaylightWhenWorkinghoursIsNight()
        {
            var (service, motionEvents, scheduler, lampDictionary) = new LightAutomationServiceBuilder().WithAreaDescriptor(new AreaDescriptor { WorkingTime = WorkingTime.AfterDusk }).WithMotion
            (
              OnNext(Time.Tics(500), new MotionEnvelope(Detectors.toiletDetector)),
              OnNext(Time.Tics(1500), new MotionEnvelope(Detectors.kitchenDetector)),
              OnNext(Time.Tics(2000), new MotionEnvelope(Detectors.livingRoomDetector))
            ).Build();

            SystemTime.Set(TimeSpan.FromHours(12));

            service.Start();
            scheduler.AdvanceToEnd(motionEvents);

            Assert.AreEqual(false, lampDictionary[Detectors.toiletDetector].IsTurnedOn);
            Assert.AreEqual(false, lampDictionary[Detectors.kitchenDetector].IsTurnedOn);
            Assert.AreEqual(false, lampDictionary[Detectors.livingRoomDetector].IsTurnedOn);
        }

        [TestMethod]
        public void MoveInRoomShouldNotTurnOnLightWhenAutomationIsDisabled()
        {
            var (service, motionEvents, scheduler, lampDictionary) = new LightAutomationServiceBuilder().WithMotion
            (
              OnNext(Time.Tics(500), new MotionEnvelope(Detectors.toiletDetector))
            ).Build();

            service.DisableAutomation(Detectors.toiletDetector);
            service.Start();
            scheduler.AdvanceToEnd(motionEvents);

            Assert.AreEqual(false, lampDictionary[Detectors.toiletDetector].IsTurnedOn);
            Assert.AreEqual(true, service.IsAutomationDisabled(Detectors.toiletDetector));
        }

        [TestMethod]
        public void MoveInRoomShouldTurnOnLightWhenAutomationIsReEnabled()
        {
            var (service, motionEvents, scheduler, lampDictionary) = new LightAutomationServiceBuilder().WithMotion
            (
              OnNext(Time.Tics(500), new MotionEnvelope(Detectors.toiletDetector)),
              OnNext(Time.Tics(2500), new MotionEnvelope(Detectors.toiletDetector))
            ).Build();

            service.DisableAutomation(Detectors.toiletDetector);
            motionEvents.Subscribe(x => service.EnableAutomation(Detectors.toiletDetector));
            service.Start();
            scheduler.AdvanceToEnd(motionEvents);

            Assert.AreEqual(true, lampDictionary[Detectors.toiletDetector].IsTurnedOn);
        }

        [TestMethod]
        public void AnalyzeMoveShouldCountPeopleNumberInRoom()
        {
            var (service, motionEvents, scheduler, lampDictionary) = new LightAutomationServiceBuilder().WithMotion
            (
              OnNext(Time.Tics(500), new MotionEnvelope(Detectors.toiletDetector)),
              OnNext(Time.Tics(1500), new MotionEnvelope(Detectors.hallwayDetectorToilet)),
              OnNext(Time.Tics(2000), new MotionEnvelope(Detectors.kitchenDetector)),
              OnNext(Time.Tics(2500), new MotionEnvelope(Detectors.livingRoomDetector)),
              OnNext(Time.Tics(3000), new MotionEnvelope(Detectors.hallwayDetectorLivingRoom)),
              OnNext(Time.Tics(3500), new MotionEnvelope(Detectors.hallwayDetectorToilet)),
              OnNext(Time.Tics(4000), new MotionEnvelope(Detectors.kitchenDetector))
            ).Build();

            service.Start();
            scheduler.AdvanceTo(service.Configuration.ConfusionResolutionTime + TimeSpan.FromMilliseconds(6000));

            Assert.AreEqual(2, service.GetCurrentNumberOfPeople(Detectors.kitchenDetector));
        }

        [TestMethod]
        public void AnalyzeMoveShouldCountPeopleNumberInHouse()
        {
            var (service, motionEvents, scheduler, lampDictionary) = new LightAutomationServiceBuilder().WithMotion
            (
              OnNext(Time.Tics(500), new MotionEnvelope(Detectors.toiletDetector)),
              OnNext(Time.Tics(1500), new MotionEnvelope(Detectors.hallwayDetectorToilet)),
              OnNext(Time.Tics(2000), new MotionEnvelope(Detectors.kitchenDetector)),
              OnNext(Time.Tics(2500), new MotionEnvelope(Detectors.livingRoomDetector)),
              OnNext(Time.Tics(3000), new MotionEnvelope(Detectors.hallwayDetectorLivingRoom)),
              OnNext(Time.Tics(3500), new MotionEnvelope(Detectors.hallwayDetectorToilet)),
              OnNext(Time.Tics(4000), new MotionEnvelope(Detectors.kitchenDetector))
            ).Build();

            service.Start();
            scheduler.AdvanceTo(service.Configuration.ConfusionResolutionTime + TimeSpan.FromMilliseconds(6000));

            Assert.AreEqual(2, service.NumberOfPersonsInHouse);
        }

        [TestMethod]
        public void WhenLeaveFromOnePersonRoomWithNoConfusionShouldTurnOffLightImmediately()
        {
            var (service, motionEvents, scheduler, lampDictionary) = new LightAutomationServiceBuilder().WithMotion
            (
              OnNext(Time.Tics(500), new MotionEnvelope(Detectors.toiletDetector)),
              OnNext(Time.Tics(1500), new MotionEnvelope(Detectors.hallwayDetectorToilet))
            ).Build();

            service.Start();
            scheduler.AdvanceJustAfterEnd(motionEvents);

            Assert.AreEqual(false, lampDictionary[Detectors.toiletDetector].IsTurnedOn);
        }

        [TestMethod]
        public void WhenLeaveFromOnePersonRoomWithConfusionShouldTurnOffWhenConfusionResolved()
        {
            var (service, motionEvents, scheduler, lampDictionary) = new LightAutomationServiceBuilder().WithMotion
            (
                  OnNext(Time.Tics(500), new MotionEnvelope(Detectors.toiletDetector)),
                  OnNext(Time.Tics(1000), new MotionEnvelope(Detectors.kitchenDetector)),
                  OnNext(Time.Tics(1500), new MotionEnvelope(Detectors.hallwayDetectorToilet)),
                  OnNext(Time.Tics(2000), new MotionEnvelope(Detectors.hallwayDetectorLivingRoom)),
                  OnNext(Time.Tics(3000), new MotionEnvelope(Detectors.kitchenDetector))
            ).Build();

            service.Start();
            scheduler.AdvanceTo(service.Configuration.ConfusionResolutionTime + TimeSpan.FromMilliseconds(1500));

            Assert.AreEqual(false, lampDictionary[Detectors.toiletDetector].IsTurnedOn);
        }

        [TestMethod]
        public void WhenLeaveFromRoomWithNoConfusionShouldTurnOffLightAfterSomeTime()
        {
            var (service, motionEvents, scheduler, lampDictionary) = new LightAutomationServiceBuilder().WithMotion
            (
              OnNext(Time.Tics(500), new MotionEnvelope(Detectors.kitchenDetector)),
              OnNext(Time.Tics(1500), new MotionEnvelope(Detectors.hallwayDetectorToilet))
            ).Build();

            service.Start();

            scheduler.AdvanceJustAfterEnd(motionEvents);
            Assert.AreEqual(true, lampDictionary[Detectors.kitchenDetector].IsTurnedOn);
            scheduler.AdvanceTo(Time.Tics(2500));
            Assert.AreEqual(false, lampDictionary[Detectors.kitchenDetector].IsTurnedOn);
        }

        [TestMethod]
        public void WhenNoMoveInRoomShouldTurnOffAfterTurnOffTimeout()
        {
            var (service, motionEvents, scheduler, lampDictionary) = new LightAutomationServiceBuilder().WithMotion
            (
                OnNext(Time.Tics(500), new MotionEnvelope(Detectors.kitchenDetector))
            ).Build();

            service.Start();
            var area = service.GetAreaDescriptor(Detectors.kitchenDetector);

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
        public void MoveInFirstPathShouldNotConfusedNextPathWhenItIsSure()
        {
            var (service, motionEvents, scheduler, lampDictionary) = new LightAutomationServiceBuilder().WithMotion
            (
                // First path
                OnNext(Time.Tics(500), new MotionEnvelope(Detectors.toiletDetector)),
                OnNext(Time.Tics(1500), new MotionEnvelope(Detectors.hallwayDetectorToilet)),
                OnNext(Time.Tics(2000), new MotionEnvelope(Detectors.kitchenDetector)),
                // Second path
                OnNext(Time.Tics(2500), new MotionEnvelope(Detectors.livingRoomDetector)),
                OnNext(Time.Tics(3000), new MotionEnvelope(Detectors.hallwayDetectorLivingRoom))
            ).Build();

            service.Start();
            scheduler.AdvanceJustAfterEnd(motionEvents);

            Assert.AreEqual(0, service.NumberOfConfusions);
            Assert.AreEqual(true, lampDictionary[Detectors.kitchenDetector].IsTurnedOn);
        }

        [TestMethod]
        public void WhenCrossPassingNumberOfPeopleSlouldBeCorrect()
        {
            var (service, motionEvents, scheduler, lampDictionary) = new LightAutomationServiceBuilder().WithMotion
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

            service.Start();
            scheduler.AdvanceJustAfterEnd(motionEvents);

            Assert.AreEqual(1, service.GetCurrentNumberOfPeople(Detectors.kitchenDetector));
            Assert.AreEqual(1, service.GetCurrentNumberOfPeople(Detectors.livingRoomDetector));
            Assert.AreEqual(2, service.NumberOfPersonsInHouse);
        }

        [TestMethod]
        public void ManualLightChangeShouldInvokeDecoderAction()
        {
            var (service, motionEvents, scheduler, lampDictionary) = new LightAutomationServiceBuilder().WithSampleDecoder(() => new IEventDecoder[] { new DisableAutomationDecoder() })
                                                                                                        .WithLampEvents
                                                                                                        (
                                                                                                            OnNext(Time.Tics(500), PowerStateChangeEvent.Create(true, Detectors.kitchenDetector, PowerStateChangeEvent.ManualSource)),
                                                                                                            OnNext(Time.Tics(1500), PowerStateChangeEvent.Create(false, Detectors.kitchenDetector, PowerStateChangeEvent.ManualSource)),
                                                                                                            OnNext(Time.Tics(2000), PowerStateChangeEvent.Create(true, Detectors.kitchenDetector, PowerStateChangeEvent.ManualSource))
                                                                                                        )
                                                                                                        .Build();
            
            service.Start();
            scheduler.AdvanceTo(service.Configuration.ManualCodeWindow.Add(TimeSpan.FromMilliseconds(500)));

            Assert.AreEqual(true, service.IsAutomationDisabled(Detectors.kitchenDetector));
        }

        [TestMethod]
        public void ManualLightChangeShouldNotInvokeDecoderActionWhenIncomplete()
        {
            var (service, motionEvents, scheduler, lampDictionary) = new LightAutomationServiceBuilder().WithSampleDecoder(() => new IEventDecoder[] { new DisableAutomationDecoder() })
                                                                                                        .WithLampEvents
                                                                                                        (
                                                                                                            OnNext(Time.Tics(500), PowerStateChangeEvent.Create(true, Detectors.kitchenDetector, PowerStateChangeEvent.ManualSource)),
                                                                                                            OnNext(Time.Tics(1500), PowerStateChangeEvent.Create(false, Detectors.kitchenDetector, PowerStateChangeEvent.ManualSource))
                                                                                                        )
                                                                                                        .Build();

            service.Start();
            scheduler.AdvanceTo(service.Configuration.ManualCodeWindow.Add(TimeSpan.FromMilliseconds(500)));

            Assert.AreEqual(false, service.IsAutomationDisabled(Detectors.kitchenDetector));
        }

        [TestMethod]
        public void ManualLightChangeShouldNotInvokeDecoderActionWhenToLong()
        {
            var (service, motionEvents, scheduler, lampDictionary) = new LightAutomationServiceBuilder().WithSampleDecoder(() => new IEventDecoder[] { new DisableAutomationDecoder() })
                                                                                                        .WithLampEvents
                                                                                                        (
                                                                                                            OnNext(Time.Tics(500), PowerStateChangeEvent.Create(true, Detectors.kitchenDetector, PowerStateChangeEvent.ManualSource)),
                                                                                                            OnNext(Time.Tics(1500), PowerStateChangeEvent.Create(false, Detectors.kitchenDetector, PowerStateChangeEvent.ManualSource)),
                                                                                                            OnNext(Time.Tics(4000), PowerStateChangeEvent.Create(true, Detectors.kitchenDetector, PowerStateChangeEvent.ManualSource))
                                                                                                        )
                                                                                                        .Build();
            
            service.Start();
            scheduler.AdvanceTo(service.Configuration.ManualCodeWindow.Add(TimeSpan.FromMilliseconds(500)));

            Assert.AreEqual(false, service.IsAutomationDisabled(Detectors.kitchenDetector));
        }

        [TestMethod]
        public void WhenTurnOnJustAfterTurnOffServiceShouldIncreaseTurnOffTimeout()
        {
            var (service, motionEvents, scheduler, lampDictionary) = new LightAutomationServiceBuilder().WithMotion
            (
                OnNext(Time.Tics(500), new MotionEnvelope(Detectors.kitchenDetector)),
                OnNext(Time.Tics(12000), new MotionEnvelope(Detectors.kitchenDetector))
            ).Build();

            service.Start();
            scheduler.AdvanceJustAfterEnd(motionEvents);

            Assert.AreEqual(false, service.IsAutomationDisabled(Detectors.kitchenDetector));
        }

        [TestMethod]
        public void WhenNoConfusionMoveThroughManyRoomsShouldTurnOfLightImmediately()
        {
            var (service, motionEvents, scheduler, lampDictionary) = new LightAutomationServiceBuilder().WithMotion
            (
                OnNext(Time.Tics(1000), new MotionEnvelope(Detectors.livingRoomDetector)),
                OnNext(Time.Tics(2000), new MotionEnvelope(Detectors.hallwayDetectorLivingRoom)),
                OnNext(Time.Tics(3000), new MotionEnvelope(Detectors.hallwayDetectorToilet)),
                OnNext(Time.Tics(4000), new MotionEnvelope(Detectors.kitchenDetector))
            ).Build();

            service.Start();
            scheduler.AdvanceJustAfter(TimeSpan.FromSeconds(2));
            Assert.AreEqual(true, lampDictionary[Detectors.livingRoomDetector].IsTurnedOn);
            Assert.AreEqual(true, lampDictionary[Detectors.hallwayDetectorLivingRoom].IsTurnedOn);
            scheduler.AdvanceJustAfter(TimeSpan.FromSeconds(3));
            Assert.AreEqual(false, lampDictionary[Detectors.livingRoomDetector].IsTurnedOn);
            Assert.AreEqual(true, lampDictionary[Detectors.hallwayDetectorLivingRoom].IsTurnedOn);
            Assert.AreEqual(true, lampDictionary[Detectors.hallwayDetectorToilet].IsTurnedOn);
            scheduler.AdvanceJustAfter(TimeSpan.FromSeconds(4));
            Assert.AreEqual(false, lampDictionary[Detectors.livingRoomDetector].IsTurnedOn);
            Assert.AreEqual(false, lampDictionary[Detectors.hallwayDetectorLivingRoom].IsTurnedOn);
            Assert.AreEqual(true, lampDictionary[Detectors.hallwayDetectorToilet].IsTurnedOn);
            Assert.AreEqual(true, lampDictionary[Detectors.kitchenDetector].IsTurnedOn);
            scheduler.AdvanceJustAfter(TimeSpan.FromSeconds(5));
            Assert.AreEqual(false, lampDictionary[Detectors.livingRoomDetector].IsTurnedOn);
            Assert.AreEqual(false, lampDictionary[Detectors.hallwayDetectorLivingRoom].IsTurnedOn);
            Assert.AreEqual(false, lampDictionary[Detectors.hallwayDetectorToilet].IsTurnedOn);
            Assert.AreEqual(true, lampDictionary[Detectors.kitchenDetector].IsTurnedOn);
        }

        [TestMethod]
        public void WhenTwoPersonsStartsFromOneRoomAndSplitToTwoOthersNumbersOfPeopleShouldBeCorrect()
        {
            var (service, motionEvents, scheduler, lampDictionary) = new LightAutomationServiceBuilder().WithMotion
            (
                OnNext(Time.Tics(1000), new MotionEnvelope(Detectors.livingRoomDetector)),
                OnNext(Time.Tics(2000), new MotionEnvelope(Detectors.hallwayDetectorLivingRoom)),
                OnNext(Time.Tics(2900), new MotionEnvelope(Detectors.bathroomDetector)),
                OnNext(Time.Tics(3000), new MotionEnvelope(Detectors.hallwayDetectorToilet)),
                OnNext(Time.Tics(4000), new MotionEnvelope(Detectors.kitchenDetector))
            ).Build();

            service.Start();

            scheduler.AdvanceJustAfter(TimeSpan.FromSeconds(4));
            Assert.AreEqual(1, service.GetCurrentNumberOfPeople(Detectors.kitchenDetector));
            Assert.AreEqual(1, service.GetCurrentNumberOfPeople(Detectors.bathroomDetector));
            Assert.AreEqual(2, service.NumberOfPersonsInHouse);
            Assert.AreEqual(true, lampDictionary[Detectors.kitchenDetector].IsTurnedOn);
            Assert.AreEqual(true, lampDictionary[Detectors.bathroomDetector].IsTurnedOn);
            Assert.AreEqual(false, lampDictionary[Detectors.hallwayDetectorLivingRoom].IsTurnedOn);
            Assert.AreEqual(false, lampDictionary[Detectors.livingRoomDetector].IsTurnedOn);
            scheduler.AdvanceJustAfter(TimeSpan.FromSeconds(5));
            Assert.AreEqual(false, lampDictionary[Detectors.hallwayDetectorToilet].IsTurnedOn);
        }

        [TestMethod]
        public void MoveCloseToRoomWithOtherPersonShouldConfuzeVectorsNearThatRoom()
        {
            var (service, motionEvents, scheduler, lampDictionary) = new LightAutomationServiceBuilder().WithMotion
            (
                //L->HL->HT->K vs move in B
                OnNext(Time.Tics(1000), new MotionEnvelope(Detectors.livingRoomDetector)),
                OnNext(Time.Tics(1500), new MotionEnvelope(Detectors.bathroomDetector)),
                OnNext(Time.Tics(2000), new MotionEnvelope(Detectors.hallwayDetectorLivingRoom)),
                OnNext(Time.Tics(2900), new MotionEnvelope(Detectors.bathroomDetector)),
                OnNext(Time.Tics(3000), new MotionEnvelope(Detectors.hallwayDetectorToilet)),
                OnNext(Time.Tics(4000), new MotionEnvelope(Detectors.kitchenDetector)),
                OnNext(Time.Tics(4100), new MotionEnvelope(Detectors.bathroomDetector))
            ).Build();

            service.Start();

            scheduler.AdvanceJustAfter(TimeSpan.FromSeconds(9));

            Assert.AreEqual(2, service.NumberOfConfusions);
            Assert.AreEqual(true, lampDictionary[Detectors.bathroomDetector].IsTurnedOn);
            Assert.AreEqual(true, lampDictionary[Detectors.kitchenDetector].IsTurnedOn);
            Assert.AreEqual(false, lampDictionary[Detectors.livingRoomDetector].IsTurnedOn);
            Assert.AreEqual(false, lampDictionary[Detectors.hallwayDetectorLivingRoom].IsTurnedOn);
            Assert.AreEqual(false, lampDictionary[Detectors.hallwayDetectorToilet].IsTurnedOn);
        }
    }
}