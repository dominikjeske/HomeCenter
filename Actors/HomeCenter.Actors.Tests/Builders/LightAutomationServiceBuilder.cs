using AutoMapper;
using HomeCenter.Model.Actors;
using HomeCenter.Model.Core;
using HomeCenter.Model.Messages.Events.Device;
using HomeCenter.Services.Configuration.DTO;
using HomeCenter.Services.MotionService.Model;
using Microsoft.Extensions.Logging;
using Microsoft.Reactive.Testing;
using Moq;
using Quartz;
using SimpleInjector;
using System;
using System.Collections.Generic;
using System.Reactive;

namespace HomeCenter.Services.MotionService.Tests
{
    public class LightAutomationServiceBuilder
    {
        private AreaDescriptor _areaDescriptor;

        // private Func<IEventDecoder[]> _sampleDecoder;
        private List<Recorded<Notification<MotionEnvelope>>> _motionEvents = new List<Recorded<Notification<MotionEnvelope>>>();

        private List<Recorded<Notification<PowerStateChangeEvent>>> _lampEvents = new List<Recorded<Notification<PowerStateChangeEvent>>>();
        private int _timeDuration = 20;
        private Container _container;

        public LightAutomationServiceBuilder WithAreaDescriptor(AreaDescriptor areaDescriptor)
        {
            _areaDescriptor = areaDescriptor;
            return this;
        }

        //public LightAutomationServiceBuilder WithSampleDecoder(Func<IEventDecoder[]> sampleDecoder)
        //{
        //    _sampleDecoder = sampleDecoder;
        //    return this;
        //}

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

            _container = new Container();
            Mapper.Initialize(p =>
            {
                p.CreateMap(typeof(ServiceDTO), typeof(LightAutomationServiceProxy)).ConstructUsingServiceLocator();
                p.CreateMap<AttachedPropertyDTO, AttachedProperty>();

                p.ShouldMapProperty = propInfo => (propInfo.CanWrite && propInfo.GetGetMethod(true).IsPublic) || propInfo.IsDefined(typeof(MapAttribute), false);
                p.ConstructServicesUsing(_container.GetInstance);
            });

            var scheduler = new TestScheduler();
            var concurrencyProvider = new TestConcurrencyProvider(scheduler);
            var quartzScheduler = Mock.Of<IScheduler>();
            var motionEvents = scheduler.CreateColdObservable<MotionEnvelope>(_motionEvents.ToArray());
            var messageBroker = new FakeMessageBroker(motionEvents);

            _container.RegisterInstance<IScheduler>(quartzScheduler);
            _container.RegisterInstance<IConcurrencyProvider>(concurrencyProvider);
            _container.RegisterInstance<ILogger<LightAutomationServiceProxy>>(new FakeLogger<LightAutomationServiceProxy>(scheduler));
            _container.RegisterInstance<IMessageBroker>(messageBroker);

            //TODO
            //Mock.Get(observableTimer).Setup(x => x.GenerateTime(motionConfiguration.PeriodicCheckTime)).Returns(scheduler.CreateColdObservable(GenerateTestTime(TimeSpan.FromSeconds(_timeDuration), motionConfiguration.PeriodicCheckTime)));

            var serviceDto = ConfigureRooms(lampDictionary);

            var lightAutomation = Mapper.Map<ServiceDTO, LightAutomationServiceProxy>(serviceDto);

            return
            (
                lightAutomation,
                motionEvents,
                scheduler,
                lampDictionary
            );
        }

        private ServiceDTO ConfigureRooms(Dictionary<string, FakeMotionLamp> lamps)
        {
            var serviceDto = new ServiceDTO();

            //toiletArea.MaxPersonCapacity = 1;

            serviceDto.ComponentsAttachedProperties.Add(new AttachedPropertyDTO
            {
                AttachedActor = Detectors.hallwayDetectorToilet,
                AttachedArea = "",
                Properties = new Dictionary<string, string>
                {
                    [MotionProperties.Neighbors] = string.Join(", ", new[] { Detectors.hallwayDetectorLivingRoom, Detectors.kitchenDetector, Detectors.staircaseDetector, Detectors.toiletDetector }),
                    [MotionProperties.Lamp] = lamps[Detectors.hallwayDetectorToilet].Id
                }
            });
            serviceDto.ComponentsAttachedProperties.Add(new AttachedPropertyDTO
            {
                AttachedActor = Detectors.hallwayDetectorLivingRoom,
                AttachedArea = "",
                Properties = new Dictionary<string, string>
                {
                    [MotionProperties.Neighbors] = string.Join(", ", new[] { Detectors.livingRoomDetector, Detectors.bathroomDetector, Detectors.hallwayDetectorToilet }),
                    [MotionProperties.Lamp] = lamps[Detectors.hallwayDetectorLivingRoom].Id
                }
            });
            serviceDto.ComponentsAttachedProperties.Add(new AttachedPropertyDTO
            {
                AttachedActor = Detectors.livingRoomDetector,
                AttachedArea = "",
                Properties = new Dictionary<string, string>
                {
                    [MotionProperties.Neighbors] = string.Join(", ", new[] { Detectors.balconyDetector, Detectors.hallwayDetectorLivingRoom }),
                    [MotionProperties.Lamp] = lamps[Detectors.livingRoomDetector].Id
                }
            });
            serviceDto.ComponentsAttachedProperties.Add(new AttachedPropertyDTO
            {
                AttachedActor = Detectors.balconyDetector,
                AttachedArea = "",
                Properties = new Dictionary<string, string>
                {
                    [MotionProperties.Neighbors] = string.Join(", ", new[] { Detectors.livingRoomDetector }),
                    [MotionProperties.Lamp] = lamps[Detectors.balconyDetector].Id
                }
            });
            serviceDto.ComponentsAttachedProperties.Add(new AttachedPropertyDTO
            {
                AttachedActor = Detectors.kitchenDetector,
                AttachedArea = "",
                Properties = new Dictionary<string, string>
                {
                    [MotionProperties.Neighbors] = string.Join(", ", new[] { Detectors.hallwayDetectorToilet }),
                    [MotionProperties.Lamp] = lamps[Detectors.kitchenDetector].Id
                }
            });
            serviceDto.ComponentsAttachedProperties.Add(new AttachedPropertyDTO
            {
                AttachedActor = Detectors.bathroomDetector,
                AttachedArea = "",
                Properties = new Dictionary<string, string>
                {
                    [MotionProperties.Neighbors] = string.Join(", ", new[] { Detectors.hallwayDetectorLivingRoom }),
                    [MotionProperties.Lamp] = lamps[Detectors.bathroomDetector].Id
                }
            });
            serviceDto.ComponentsAttachedProperties.Add(new AttachedPropertyDTO
            {
                AttachedActor = Detectors.badroomDetector,
                AttachedArea = "",
                Properties = new Dictionary<string, string>
                {
                    [MotionProperties.Neighbors] = string.Join(", ", new[] { Detectors.hallwayDetectorLivingRoom }),
                    [MotionProperties.Lamp] = lamps[Detectors.badroomDetector].Id
                }
            });
            serviceDto.ComponentsAttachedProperties.Add(new AttachedPropertyDTO
            {
                AttachedActor = Detectors.staircaseDetector,
                AttachedArea = "",
                Properties = new Dictionary<string, string>
                {
                    [MotionProperties.Neighbors] = string.Join(", ", new[] { Detectors.hallwayDetectorToilet }),
                    [MotionProperties.Lamp] = lamps[Detectors.staircaseDetector].Id
                }
            });
            serviceDto.ComponentsAttachedProperties.Add(new AttachedPropertyDTO
            {
                AttachedActor = Detectors.toiletDetector,
                AttachedArea = "",
                Properties = new Dictionary<string, string>
                {
                    [MotionProperties.Neighbors] = string.Join(", ", new[] { Detectors.hallwayDetectorToilet }),
                    [MotionProperties.Lamp] = lamps[Detectors.toiletDetector].Id
                }
            });

            return serviceDto;
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