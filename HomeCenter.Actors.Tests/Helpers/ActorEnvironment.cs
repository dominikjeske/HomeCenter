using HomeCenter.Abstractions;
using HomeCenter.Actors.Tests.Fakes;
using HomeCenter.Messages.Queries.Services;
using HomeCenter.Services.MotionService;
using Microsoft.Reactive.Testing;
using Proto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HomeCenter.Actors.Tests.Helpers
{
    internal class ActorEnvironment : IDisposable
    {
        private const int JUST_TIME = 600;
        private readonly TestScheduler _scheduler;
        private readonly ITestableObservable<MotionEnvelope> _motionEvents;
        private readonly Dictionary<string, FakeMotionLamp> _lamps;
        private readonly IDisposable? _externalResources;
        private readonly ActorSystem _system = new();
        private readonly RootContext _context;
        private readonly PID _pid;
        private readonly string _serviceProcessName;

        public TestScheduler Scheduler => _scheduler;

        public ActorEnvironment(TestScheduler Scheduler, ITestableObservable<MotionEnvelope> MotionEvents, Dictionary<string, FakeMotionLamp> Lamps, LightAutomationServiceProxy actor, IDisposable? externalResources)
        {
            _scheduler = Scheduler;
            _motionEvents = MotionEvents;

            _lamps = Lamps;
            _externalResources = externalResources;
            _serviceProcessName = $"motionService_{Guid.NewGuid()}";
            _context = new RootContext(_system);
            _pid = _context.SpawnNamed(Props.FromProducer(() => actor), _serviceProcessName);
        }

        public void Send(Command actorMessage)
        {
            _context.Send(_pid, actorMessage);
            IsAlive();
        }

        public Task<R> Query<R>(Query actorMessage)
        {
            return _context.RequestAsync<R>(_pid, actorMessage);
        }

        /// <summary>
        /// Allows to wait for actor to process previous task
        /// </summary>
        public bool IsAlive()
        {
            return _context.RequestAsync<bool>(new PID("nonhost", _serviceProcessName), IsAliveQuery.Default).GetAwaiter().GetResult();
        }

        public void Dispose()
        {
            _context.StopAsync(_pid);
            _externalResources?.Dispose();
        }

        public void AdvanceToEnd(TimeSpan? timeAfter = null)
        {
            var motionEnd = TimeSpan.FromTicks(_motionEvents.Messages.Max(x => x.Time));
            long deleay = (long)(timeAfter.HasValue ? timeAfter.Value.TotalMilliseconds : 0);

            Scheduler.AdvanceTo(motionEnd, deleay, true);
        }

        public void AdvanceTo(TimeSpan time, bool justAfter = false) => Scheduler.AdvanceTo(time, justAfter ? JUST_TIME : 0, true);

        public void AdvanceToIndex(int index, TimeSpan delay, bool justAfter = false)
        {
            var timeAfter = justAfter ? delay + TimeSpan.FromMilliseconds(JUST_TIME) : delay;
            Scheduler.AdvanceTo(GetMotionTime(index), (long)timeAfter.TotalMilliseconds, true);
        }

        public void AdvanceJustBefore(TimeSpan time) => Scheduler.AdvanceTo(time - TimeSpan.FromMilliseconds(500));

        public bool LampState(string lamp) => _lamps[lamp].IsTurnedOn;

        public void SendCommand(Command command) => Send(command);

        public void SendAfterFirstMove(Action<MotionEnvelope> action) => _motionEvents.Subscribe(action);

        private TimeSpan GetMotionTime(int vectorIndex) => TimeSpan.FromTicks(_motionEvents.Messages[vectorIndex].Time);
    }
}