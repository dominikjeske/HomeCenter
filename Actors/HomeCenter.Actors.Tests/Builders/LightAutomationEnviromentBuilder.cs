using AutoMapper;
using HomeCenter.Model.Actors;
using HomeCenter.Model.Core;
using HomeCenter.Model.Messages.Events.Device;
using HomeCenter.Services.Configuration.DTO;
using HomeCenter.Utils.LogProviders;
using Microsoft.Extensions.Logging;
using Microsoft.Reactive.Testing;
using SimpleInjector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;

namespace HomeCenter.Services.MotionService.Tests
{
    internal class LightAutomationEnviromentBuilder
    {
        private readonly ServiceDTO _serviceConfig;
        private readonly TestScheduler _scheduler = new TestScheduler();
        private readonly Container _container = new Container();
        private readonly List<Recorded<Notification<MotionEnvelope>>> _motionEvents = new List<Recorded<Notification<MotionEnvelope>>>();
        private readonly List<Recorded<Notification<PowerStateChangeEvent>>> _lampEvents = new List<Recorded<Notification<PowerStateChangeEvent>>>();

        private int? _timeDuration;
        private TimeSpan? _periodicCheckTime;

        public LightAutomationEnviromentBuilder(ServiceDTO serviceConfig)
        {
            _serviceConfig = serviceConfig;
        }

        public LightAutomationEnviromentBuilder WithMotion(params Recorded<Notification<MotionEnvelope>>[] messages)
        {
            _motionEvents.AddRange(messages);
            return this;
        }

        public LightAutomationEnviromentBuilder WithMotions(Dictionary<int, string> motions)
        {
            _motionEvents.AddRange(motions.Select(x => new Recorded<Notification<MotionEnvelope>>(Time.Tics(x.Key), Notification.CreateOnNext(new MotionEnvelope(x.Value)))));

            return this;
        }

        public LightAutomationEnviromentBuilder WithMotions(List<Tuple<int, string>> motions)
        {
            _motionEvents.AddRange(motions.Select(x => new Recorded<Notification<MotionEnvelope>>(Time.Tics(x.Item1), Notification.CreateOnNext(new MotionEnvelope(x.Item2)))));

            return this;
        }

        public LightAutomationEnviromentBuilder WithRepeatedMotions(string roomUid, int numberOfMotions, TimeSpan waitTime)
        {
            long ticks = 0;

            for (int i = 0; i < numberOfMotions; i++)
            {
                ticks += Time.Tics((int)waitTime.TotalMilliseconds);

                _motionEvents.Add(new Recorded<Notification<MotionEnvelope>>(ticks, Notification.CreateOnNext(new MotionEnvelope(roomUid))));
            }

            return this;
        }

        /// <summary>
        /// Add repeat motion to <paramref name="roomUid"/> that takes <paramref name="motionTime"/> and waits between moves <paramref name="waitTime"/>
        /// </summary>
        /// <param name="roomUid"></param>
        /// <param name="motionTime"></param>
        /// <param name="waitTime">Deafault value is 3 seconds</param>
        /// <returns></returns>
        public LightAutomationEnviromentBuilder WithRepeatedMotions(string roomUid, TimeSpan motionTime, TimeSpan? waitTime = null)
        {
            var time = waitTime ?? TimeSpan.FromSeconds(3);

            int num = (int)(motionTime.TotalMilliseconds / time.TotalMilliseconds);

            WithRepeatedMotions(roomUid, num, time);

            return this;
        }

        public LightAutomationEnviromentBuilder WithLampEvents(params Recorded<Notification<PowerStateChangeEvent>>[] messages)
        {
            _lampEvents.AddRange(messages);
            return this;
        }

        public LightAutomationEnviromentBuilder WithPeriodicCheckTime(TimeSpan periodicCheckTimw)
        {
            _periodicCheckTime = periodicCheckTimw;
            return this;
        }

        public LightAutomationEnviromentBuilder WithTimeDuration(int timeDuration)
        {
            _timeDuration = timeDuration;
            return this;
        }

        public ActorEnvironment Build()
        {
            var lampDictionary = CreateFakeLamps();
            var motionEvents = _scheduler.CreateColdObservable(_motionEvents.ToArray());
            var messageBroker = new FakeMessageBroker(motionEvents, lampDictionary);

            var (mapper, logger) = Bootstrap(messageBroker);
            var actor = mapper.Map<ServiceDTO, LightAutomationServiceProxy>(_serviceConfig);
            logger.InitLogger();

            var actorContext = new ActorEnvironment(_scheduler, motionEvents, lampDictionary, logger, actor);
            actorContext.IsAlive();

            return actorContext;
        }

        private (IMapper, FakeLogger<LightAutomationServiceProxy>) Bootstrap(IMessageBroker messageBroker)
        {
            MapperConfiguration config = ConfigureMapper();
            var logger = new FakeLogger<LightAutomationServiceProxy>(_scheduler);

            var concurrencyProvider = new TestConcurrencyProvider(_scheduler);
            _container.RegisterInstance<IConcurrencyProvider>(concurrencyProvider);
            _container.RegisterInstance<ILogger<LightAutomationServiceProxy>>(logger);
            _container.RegisterInstance(messageBroker);

            return (config.CreateMapper(), logger);
        }

        private ILoggerProvider[] GetLogProviders()
        {
            return new ILoggerProvider[] { new ConsoleLogProvider() };
        }

        private void RegisterLogging()
        {
            var loggerOptions = new LoggerFilterOptions { MinLevel = LogLevel.Debug };
            var loggerFactory = new LoggerFactory(GetLogProviders(), loggerOptions);

            _container.RegisterInstance<ILoggerFactory>(loggerFactory);
            _container.Register(typeof(ILogger<>), typeof(Logger<>), Lifestyle.Singleton);
        }

        private MapperConfiguration ConfigureMapper()
        {
            return new MapperConfiguration(p =>
            {
                p.CreateMap(typeof(ServiceDTO), typeof(LightAutomationServiceProxy)).ConstructUsingServiceLocator();
                p.CreateMap<AttachedPropertyDTO, AttachedProperty>();

                p.ShouldMapProperty = propInfo => (propInfo.CanWrite && propInfo.GetGetMethod(true).IsPublic) || propInfo.IsDefined(typeof(MapAttribute), false);
                p.ConstructServicesUsing(_container.GetInstance);
            });
        }

        private Dictionary<string, FakeMotionLamp> CreateFakeLamps()
        {
            var lampDictionary = new Dictionary<string, FakeMotionLamp>();

            foreach (var detector in _serviceConfig.ComponentsAttachedProperties)
            {
                var detectorName = detector.Properties[MotionProperties.Lamp];

                lampDictionary.Add(detectorName, new FakeMotionLamp(detectorName));
            }

            return lampDictionary;
        }
    }
}