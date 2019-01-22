using AutoMapper;
using HomeCenter.Model.Actors;
using HomeCenter.Model.Core;
using HomeCenter.Model.Messages.Events.Device;
using HomeCenter.Model.Messages.Queries.Services;
using HomeCenter.Services.Configuration.DTO;
using HomeCenter.Utils.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Reactive.Testing;
using Moq;
using Proto;
using Quartz;
using SimpleInjector;
using System;
using System.Collections.Generic;
using System.Reactive;

namespace HomeCenter.Services.MotionService.Tests
{
    internal class LightAutomationServiceBuilder
    {
        private string _workingTime;
        private TimeSpan? _confusionResolutionTime;

        // private Func<IEventDecoder[]> _sampleDecoder;
        private List<Recorded<Notification<MotionEnvelope>>> _motionEvents = new List<Recorded<Notification<MotionEnvelope>>>();

        private List<Recorded<Notification<PowerStateChangeEvent>>> _lampEvents = new List<Recorded<Notification<PowerStateChangeEvent>>>();
        private int _timeDuration = 20;
        private Container _container;
        private readonly ActorContext _actorContext;

        public LightAutomationServiceBuilder(ActorContext actorContext)
        {
            _actorContext = actorContext;
        }

        public LightAutomationServiceBuilder WithWorkingTime(string wortkingTime)
        {
            _workingTime = wortkingTime;
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

        public LightAutomationServiceBuilder WithConfusionResolutionTime(TimeSpan confusionResolutionTime)
        {
            _confusionResolutionTime = confusionResolutionTime;
            return this;
        }

        public LightAutomationServiceBuilder WithTimeDuration(int timeDuration)
        {
            _timeDuration = timeDuration;
            return this;
        }

        public
        (
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



            var config = new MapperConfiguration(p =>
            {
                p.CreateMap(typeof(ServiceDTO), typeof(LightAutomationServiceProxy)).ConstructUsingServiceLocator();
                p.CreateMap<AttachedPropertyDTO, AttachedProperty>();

                p.ShouldMapProperty = propInfo => (propInfo.CanWrite && propInfo.GetGetMethod(true).IsPublic) || propInfo.IsDefined(typeof(MapAttribute), false);
                p.ConstructServicesUsing(_container.GetInstance);
            });

            var mapper = config.CreateMapper();

            var scheduler = new TestScheduler();
            var concurrencyProvider = new TestConcurrencyProvider(scheduler);
            var quartzScheduler = Mock.Of<IScheduler>();
            var motionEvents = scheduler.CreateColdObservable<MotionEnvelope>(_motionEvents.ToArray());
            var messageBroker = new FakeMessageBroker(motionEvents, lampDictionary);

            _container.RegisterInstance<IScheduler>(quartzScheduler);
            _container.RegisterInstance<IConcurrencyProvider>(concurrencyProvider);
            _container.RegisterInstance<ILogger<LightAutomationServiceProxy>>(new FakeLogger<LightAutomationServiceProxy>(scheduler));
            _container.RegisterInstance<IMessageBroker>(messageBroker);

            //TODO
            //Mock.Get(observableTimer).Setup(x => x.GenerateTime(motionConfiguration.PeriodicCheckTime)).Returns(scheduler.CreateColdObservable(GenerateTestTime(TimeSpan.FromSeconds(_timeDuration), motionConfiguration.PeriodicCheckTime)));

            var areProperties = new Dictionary<string, string>();
            if (!string.IsNullOrWhiteSpace(_workingTime))
            {
                areProperties.Add(MotionProperties.WorkingTime, _workingTime);
            }
            
            var serviceDto = ConfigureRooms(lampDictionary, areProperties);
            if (_confusionResolutionTime.HasValue)
            {
                serviceDto.Properties.Add(MotionProperties.ConfusionResolutionTime, _confusionResolutionTime.ToString());
            }

            var props = Props.FromProducer(() => mapper.Map<ServiceDTO, LightAutomationServiceProxy>(serviceDto));
            _actorContext.PID = _actorContext.Context.SpawnNamed(props, "motionService");

            _actorContext.IsAlive();
            
            return
            (
                motionEvents,
                scheduler,
                lampDictionary
            );
        }

        private ServiceDTO ConfigureRooms(Dictionary<string, FakeMotionLamp> lamps, Dictionary<string, string> areaProperties)
        {
            var serviceDto = new ServiceDTO
            {
                IsEnabled = true,
                Properties = new Dictionary<string, string>()
            };

            AddArea(serviceDto, Areas.Hallway, areaProperties);
            AddArea(serviceDto, Areas.Badroom, areaProperties);
            AddArea(serviceDto, Areas.Balcony, areaProperties);
            AddArea(serviceDto, Areas.Bathroom, areaProperties);
            AddArea(serviceDto, Areas.Kitchen, areaProperties);
            AddArea(serviceDto, Areas.Livingroom, areaProperties);
            AddArea(serviceDto, Areas.Staircase, areaProperties);
            AddArea(serviceDto, Areas.Toilet, areaProperties.AddChained(MotionProperties.MaxPersonCapacity, "1"));

            AddMotionSensor(Detectors.hallwayDetectorToilet, Areas.Hallway, new List<string> { Detectors.hallwayDetectorLivingRoom, Detectors.kitchenDetector, Detectors.staircaseDetector, Detectors.toiletDetector }, lamps, serviceDto);
            AddMotionSensor(Detectors.hallwayDetectorLivingRoom, Areas.Hallway, new List<string> { Detectors.livingRoomDetector, Detectors.bathroomDetector, Detectors.hallwayDetectorToilet }, lamps, serviceDto);
            AddMotionSensor(Detectors.livingRoomDetector, Areas.Livingroom, new List<string> { Detectors.livingRoomDetector }, lamps, serviceDto);
            AddMotionSensor(Detectors.balconyDetector, Areas.Balcony, new List<string> { Detectors.hallwayDetectorLivingRoom }, lamps, serviceDto);
            AddMotionSensor(Detectors.kitchenDetector, Areas.Kitchen, new List<string> { Detectors.hallwayDetectorToilet }, lamps, serviceDto);
            AddMotionSensor(Detectors.bathroomDetector, Areas.Bathroom, new List<string> { Detectors.hallwayDetectorLivingRoom }, lamps, serviceDto);
            AddMotionSensor(Detectors.badroomDetector, Areas.Badroom, new List<string> { Detectors.hallwayDetectorLivingRoom }, lamps, serviceDto);
            AddMotionSensor(Detectors.staircaseDetector, Areas.Staircase, new List<string> { Detectors.hallwayDetectorToilet }, lamps, serviceDto);
            AddMotionSensor(Detectors.toiletDetector, Areas.Toilet, new List<string> { Detectors.hallwayDetectorToilet }, lamps, serviceDto);

            return serviceDto;
        }

        private static void AddMotionSensor(string motionSensor, string area, IEnumerable<string> neighbors, Dictionary<string, FakeMotionLamp> lamps, ServiceDTO serviceDto)
        {
            serviceDto.ComponentsAttachedProperties.Add(new AttachedPropertyDTO
            {
                AttachedActor = motionSensor,
                AttachedArea = area,
                Properties = new Dictionary<string, string>
                {
                    [MotionProperties.Neighbors] = string.Join(", ", neighbors),
                    [MotionProperties.Lamp] = lamps[motionSensor].Id
                }
            });
        }

        private static void AddArea(ServiceDTO serviceDto, string areaName, IDictionary<string, string> properties = null)
        {
            var area = new AttachedPropertyDTO
            {
                AttachedActor = areaName,
                Properties = new Dictionary<string, string>()
            };

            if (properties != null)
            {
                foreach (var property in properties)
                {
                    area.Properties.Add(property.Key, property.Value);
                }
            }

            serviceDto.AreasAttachedProperties.Add(area);
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