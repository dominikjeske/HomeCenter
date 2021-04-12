using HomeCenter.Abstractions;
using HomeCenter.Actors.Tests.Fakes;
using HomeCenter.Actors.Tests.Helpers;
using HomeCenter.Messages.Events.Device;
using HomeCenter.Services.Actors;
using HomeCenter.Services.Configuration.DTO;
using HomeCenter.Services.MotionService;
using HomeCenter.Services.MotionService.Tests;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Reactive.Testing;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;

namespace HomeCenter.Actors.Tests.Builders
{
    internal sealed class EnviromentBuilder
    {
        private readonly TestScheduler _scheduler = new TestScheduler();
        private readonly List<Recorded<Notification<MotionEnvelope>>> _motionEvents = new();
        private readonly List<Recorded<Notification<PowerStateChangeEvent>>> _lampEvents = new();
        private int? _timeDuration;
        private TimeSpan? _periodicCheckTime;
        private ServiceDTO? _serviceConfig;

        private EnviromentBuilder()
        {
        }

        public static EnviromentBuilder Create(Action<LightServiceBuilder> action)
        {
            var builder = new EnviromentBuilder();
            return builder.WithService(action);
        }

        public EnviromentBuilder WithService(Action<LightServiceBuilder> action)
        {
            LightServiceBuilder builder = new();
            action(builder);
            _serviceConfig = builder.Build();
            return this;
        }

        public EnviromentBuilder WithMotions(Dictionary<string, string> motions)
        {
            foreach (var motion in motions)
            {
                var key = motion.Key;

                int timeIndex = key.IndexOf("/");

                if (timeIndex > -1)
                {
                    var motionStart = TimeSpan.FromMilliseconds(int.Parse(key[0..timeIndex]));
                    var motionLenght = TimeSpan.FromSeconds(int.Parse(key[(timeIndex + 1)..^0]));

                    CreateRepeatedMotions(motion.Value, motionLenght, motionStart);
                }
                else if (key.All(char.IsDigit))
                {
                    CreateMotionRecord(int.Parse(key), motion.Value);
                }
            }

            return this;
        }

        

        private void CreateMotionRecord(long miliseconds, string room)
        {
            var time = Time.Tics(miliseconds);
            Recorded < Notification < MotionEnvelope >> record = new(time,  Notification.CreateOnNext(new MotionEnvelope(room)));

            if (_motionEvents.Any(x => x.Time == time)) throw new InvalidOperationException($"Cannot add more then one event at {miliseconds}ms");

            _motionEvents.Add(record);
        }

        private EnviromentBuilder CreateRepeatedMotions(string roomUid, int numberOfMotions, TimeSpan waitTime, TimeSpan startTime, TimeSpan endTime)
        {
            var time = (long)startTime.TotalMilliseconds;

            CreateMotionRecord(time, roomUid);

            for (int i = 0; i < numberOfMotions; i++)
            {
                time += (long)waitTime.TotalMilliseconds;

                CreateMotionRecord(time, roomUid);
            }

            return this;
        }

        /// <summary>
        /// Add repeat motion to <paramref name="roomUid"/> that takes <paramref name="motionLength"/> and waits between moves <paramref name="waitTime"/>
        /// </summary>
        /// <param name="roomUid"></param>
        /// <param name="motionLength"></param>
        /// <param name="waitTime">Default value is 3 seconds</param>
        private EnviromentBuilder CreateRepeatedMotions(string roomUid, TimeSpan motionLength, TimeSpan motionStart, TimeSpan? waitTime = null)
        {
            var time = waitTime ?? TimeSpan.FromSeconds(3);

            var num = (int)(motionLength.TotalMilliseconds / time.TotalMilliseconds);

            CreateRepeatedMotions(roomUid, num, time, motionStart, motionStart + motionLength);

            return this;
        }

        public EnviromentBuilder WithLampEvents(params Recorded<Notification<PowerStateChangeEvent>>[] messages)
        {
            _lampEvents.AddRange(messages);
            return this;
        }

        public EnviromentBuilder WithPeriodicCheckTime(TimeSpan periodicCheckTimw)
        {
            _periodicCheckTime = periodicCheckTimw;
            return this;
        }

        public EnviromentBuilder WithTimeDuration(int timeDuration)
        {
            _timeDuration = timeDuration;
            return this;
        }

        public ActorEnvironment Build()
        {
            var lampDictionary = CreateFakeLamps();
            var motionEvents = _scheduler.CreateColdObservable(_motionEvents.OrderBy(x => x.Time).ToArray());
            RavenDbConfigurator? ravenDbConfigurator = null;

            var hostBuilder = new HostBuilder()
            .ConfigureServices(services =>
            {
                services.AddSingleton<IConcurrencyProvider>(new TestConcurrencyProvider(_scheduler));
                services.AddSingleton<IMessageBroker>(new FakeMessageBroker(motionEvents, lampDictionary));
                services.AddSingleton<DeviceActorMapper>();
                services.AddSingleton<BaseObjectMapper>();
                services.AddSingleton<ClassActivator>();
                services.AddSingleton<ServiceMapper>();
            });

            if (RavenConfig.UseRavenDbLogs)
            {
                ravenDbConfigurator = new RavenDbConfigurator();
                hostBuilder.UseSerilog((builder, configuration) => ravenDbConfigurator.Configure(builder, configuration, _scheduler));

                if (RavenConfig.CleanLogsBeforeRun)
                {
                    ravenDbConfigurator.Clear();
                }
            }

            var host = hostBuilder.Build();

            var sm = host.Services.Get<ServiceMapper>();

            var actor = sm.Map(_serviceConfig!, typeof(LightAutomationServiceProxy)) as LightAutomationServiceProxy;

            if (actor is null) throw new NullReferenceException($"Type not mapped to {nameof(LightAutomationServiceProxy)}");

            var actorContext = new ActorEnvironment(_scheduler, motionEvents, lampDictionary, actor, ravenDbConfigurator);
            actorContext.IsAlive();

            return actorContext;
        }

        private Dictionary<string, FakeMotionLamp> CreateFakeLamps()
        {
            var lampDictionary = new Dictionary<string, FakeMotionLamp>();

            foreach (var detector in _serviceConfig!.ComponentsAttachedProperties)
            {
                var detectorName = detector.Properties[MotionProperties.Lamp]?.ToString();
                if (detectorName is not null)
                {
                    lampDictionary.Add(detectorName, new FakeMotionLamp(detectorName));
                }
            }

            return lampDictionary;
        }
    }
}