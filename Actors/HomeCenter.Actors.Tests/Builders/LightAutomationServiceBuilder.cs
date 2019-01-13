using Force.DeepCloner;
using HomeCenter.Model.Messages.Events.Device;
using HomeCenter.Services.MotionService.Model;
using Microsoft.Reactive.Testing;
using Moq;
using System;
using System.Collections.Generic;
using System.Reactive;

namespace HomeCenter.Services.MotionService.Tests
{
    public class LightAutomationServiceBuilder
    {
        private AreaDescriptor _areaDescriptor;
        private Func<IEventDecoder[]> _sampleDecoder;
        private List<Recorded<Notification<MotionEnvelope>>> _motionEvents = new List<Recorded<Notification<MotionEnvelope>>>();
        private List<Recorded<Notification<PowerStateChangeEvent>>> _lampEvents = new List<Recorded<Notification<PowerStateChangeEvent>>>();
        private int _timeDuration = 20;

        public LightAutomationServiceBuilder WithAreaDescriptor(AreaDescriptor areaDescriptor)
        {
            _areaDescriptor = areaDescriptor;
            return this;
        }

        public LightAutomationServiceBuilder WithSampleDecoder(Func<IEventDecoder[]> sampleDecoder)
        {
            _sampleDecoder = sampleDecoder;
            return this;
        }

        public LightAutomationServiceBuilder WithMotion(params Recorded<Notification<MotionEnvelope>>[] messages)
        {
            _motionEvents.AddRange(messages);
            return this;
        }

        public LightAutomationServiceBuilder WithLampEvents(params Recorded<Notification<PowerStateChangeEvent>>[] messages)
        {
            _lampEvents.AddRange(messages);
            return this;
        }

        public LightAutomationServiceBuilder WithTimeDuration(int timeDuration)
        {
            _timeDuration = timeDuration;
            return this;
        }

        public
        (
            LightAutomationServiceProxy,
            ITestableObservable<MotionEnvelope>,
            TestScheduler,
            Dictionary<string, FakeMotionLamp>
        )
        Build()
        {
            AreaDescriptor area = _areaDescriptor ?? new AreaDescriptor();

            var hallwayLampToilet = new FakeMotionLamp(Detectors.hallwayDetectorToilet);
            var hallwayLampLivingRoom = new FakeMotionLamp(Detectors.hallwayDetectorLivingRoom);
            var toiletLamp = new FakeMotionLamp(Detectors.toiletDetector);
            var livingRoomLamp = new FakeMotionLamp(Detectors.livingRoomDetector);
            var bathroomLamp = new FakeMotionLamp(Detectors.bathroomDetector);
            var badroomLamp = new FakeMotionLamp(Detectors.badroomDetector);
            var kitchenLamp = new FakeMotionLamp(Detectors.kitchenDetector);
            var balconyLamp = new FakeMotionLamp(Detectors.balconyDetector);
            var staircaseLamp = new FakeMotionLamp(Detectors.staircaseDetector);

            var lampDictionary = new Dictionary<string, FakeMotionLamp>
            {
                { Detectors.hallwayDetectorToilet, hallwayLampToilet },
                { Detectors.hallwayDetectorLivingRoom, hallwayLampLivingRoom },
                { Detectors.toiletDetector, toiletLamp },
                { Detectors.livingRoomDetector, livingRoomLamp },
                { Detectors.bathroomDetector, bathroomLamp },
                { Detectors.badroomDetector, badroomLamp },
                { Detectors.kitchenDetector, kitchenLamp },
                { Detectors.balconyDetector, balconyLamp },
                { Detectors.staircaseDetector, staircaseLamp }
            };

            var scheduler = new TestScheduler();
            var concurrencyProvider = new TestConcurrencyProvider(scheduler);
            var motionConfigurationProvider = new MotionConfigurationProvider();
            var motionConfiguration = motionConfigurationProvider.GetConfiguration();

            var observableTimer = Mock.Of<IObservableTimer>();
            var quartzScheduler = Mock.Of<Quartz.IScheduler>();

            var logger = new FakeLogger<LightAutomationServiceProxy>(scheduler);

            Mock.Get(observableTimer).Setup(x => x.GenerateTime(motionConfiguration.PeriodicCheckTime)).Returns(scheduler.CreateColdObservable(GenerateTestTime(TimeSpan.FromSeconds(_timeDuration), motionConfiguration.PeriodicCheckTime)));

            var motionEvents = scheduler.CreateColdObservable<MotionEnvelope>(_motionEvents.ToArray());
            var messageBroker = new FakeMessageBroker(motionEvents);

            var lightAutomation = new LightAutomationServiceProxy(concurrencyProvider, motionConfigurationProvider, observableTimer, quartzScheduler, messageBroker, logger);

            var descriptors = new List<RoomInitializer>
                {
                    new RoomInitializer(Detectors.hallwayDetectorToilet, new[] { Detectors.hallwayDetectorLivingRoom, Detectors.kitchenDetector, Detectors.staircaseDetector, Detectors.toiletDetector }, hallwayLampToilet.Id, _sampleDecoder?.Invoke(), area.DeepClone()),
                    new RoomInitializer(Detectors.hallwayDetectorLivingRoom, new[] { Detectors.livingRoomDetector, Detectors.bathroomDetector, Detectors.hallwayDetectorToilet }, hallwayLampLivingRoom.Id, _sampleDecoder?.Invoke(), area.DeepClone()),
                    new RoomInitializer(Detectors.livingRoomDetector, new[] { Detectors.balconyDetector, Detectors.hallwayDetectorLivingRoom }, livingRoomLamp.Id, _sampleDecoder?.Invoke(), area.DeepClone()),
                    new RoomInitializer(Detectors.balconyDetector, new[] { Detectors.livingRoomDetector }, balconyLamp.Id, _sampleDecoder?.Invoke(), area.DeepClone()),
                    new RoomInitializer(Detectors.kitchenDetector, new[] { Detectors.hallwayDetectorToilet }, kitchenLamp.Id,  _sampleDecoder?.Invoke(), area.DeepClone()),
                    new RoomInitializer(Detectors.bathroomDetector, new[] { Detectors.hallwayDetectorLivingRoom }, bathroomLamp.Id,  _sampleDecoder?.Invoke(), area.DeepClone()),
                    new RoomInitializer(Detectors.badroomDetector, new[] { Detectors.hallwayDetectorLivingRoom }, badroomLamp.Id, _sampleDecoder?.Invoke() , area.DeepClone()),
                    new RoomInitializer(Detectors.staircaseDetector, new[] { Detectors.hallwayDetectorToilet }, staircaseLamp.Id, _sampleDecoder?.Invoke() , area.DeepClone()),
                };

            var toiletArea = area.DeepClone();
            toiletArea.MaxPersonCapacity = 1;
            descriptors.Add(new RoomInitializer(Detectors.toiletDetector, new[] { Detectors.hallwayDetectorToilet }, toiletLamp.Id, _sampleDecoder?.Invoke(), toiletArea));

            lightAutomation.RegisterRooms(descriptors);
            lightAutomation.Initialize();

            return
            (
                lightAutomation,
                motionEvents,
                scheduler,
                lampDictionary
            );
        }

        public Recorded<Notification<DateTimeOffset>>[] GenerateTestTime(TimeSpan duration, TimeSpan frequency)
        {
            var time = new List<Recorded<Notification<DateTimeOffset>>>();
            var durationSoFar = TimeSpan.FromTicks(0);
            var dateSoFar = new DateTimeOffset(1, 1, 1, 0, 0, 0, TimeSpan.FromTicks(0));
            while (true)
            {
                durationSoFar = durationSoFar.Add(frequency);
                if (durationSoFar > duration) break;

                dateSoFar = dateSoFar.Add(frequency);
                time.Add(new Recorded<Notification<DateTimeOffset>>(durationSoFar.Ticks, Notification.CreateOnNext(dateSoFar)));
            }

            return time.ToArray();
        }
    }
}