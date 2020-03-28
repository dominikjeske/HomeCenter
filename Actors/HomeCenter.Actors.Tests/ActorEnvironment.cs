using HomeCenter.Model.Messages.Commands;
using HomeCenter.Model.Messages.Queries;
using HomeCenter.Model.Messages.Queries.Services;
using Microsoft.Reactive.Testing;
using Proto;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HomeCenter.Services.MotionService.Tests
{
    internal class ActorEnvironment : IDisposable
    {
        private RootContext Context { get; } = new RootContext();
        private PID PID { get; set; }

        private TestScheduler Scheduler { get; set; }

        private FakeLogger<LightAutomationServiceProxy> Logger { get; set; }

        private ITestableObservable<MotionEnvelope> MotionEvents { get; set; }

        private Dictionary<string, FakeMotionLamp> Lamps { get; set; }

        public ActorEnvironment(TestScheduler Scheduler, ITestableObservable<MotionEnvelope> MotionEvents, Dictionary<string, FakeMotionLamp> Lamps, FakeLogger<LightAutomationServiceProxy> Logger)
        {
            this.Scheduler = Scheduler;
            this.MotionEvents = MotionEvents;
            this.Logger = Logger;
            this.Lamps = Lamps;
        }

        public void CreateService(LightAutomationServiceProxy actor)
        {
            PID = Context.SpawnNamed(Props.FromProducer(() => actor), "motionService");
        }

        public void Send(Command actorMessage)
        {
            Context.Send(PID, actorMessage);
            IsAlive();
        }

        public Task<R> Query<R>(Query actorMessage)
        {
            return Context.RequestAsync<R>(PID, actorMessage);
        }

        /// <summary>
        /// Allows to wait for actor to process previous task
        /// </summary>
        /// <returns></returns>
        public bool IsAlive()
        {
            return Context.RequestAsync<bool>(new PID("nonhost", "motionService"), IsAliveQuery.Default).GetAwaiter().GetResult();
        }

        public void Dispose()
        {
            Logger.Dispose();
            PID.StopAsync();
        }

        public void AdvanceToEnd() => Scheduler.AdvanceToEnd(MotionEvents);

        public void AdvanceTo(TimeSpan time) => Scheduler.AdvanceTo(time);

        /// <summary>
        /// We calculate next full second after given time and return just after this time (default 100ms)
        /// </summary>
        /// <param name="time"></param>
        public void AdvanceJustAfterRoundUp(TimeSpan time) => Scheduler.AdvanceJustAfter(TimeSpan.FromSeconds(Math.Ceiling(time.TotalSeconds)));

        public void AdvanceTo(long ticks) => Scheduler.AdvanceTo(ticks);

        public void AdvanceJustAfterEnd(int timeAfter = 500) => Scheduler.AdvanceJustAfterEnd(MotionEvents, timeAfter);

        public void AdvanceJustAfter(TimeSpan time) => Scheduler.AdvanceJustAfter(time);

        public void AdvanceJustBefore(TimeSpan time) => Scheduler.AdvanceJustBefore(time);

        public bool LampState(string lamp) => Lamps[lamp].IsTurnedOn;

        public void SendCommand(Command command) => Send(command);


        public void RunAfterFirstMove(Action<MotionEnvelope> action) => MotionEvents.Subscribe(action);

        public TimeSpan GetMotionTime(int vectorIndex) => TimeSpan.FromTicks(MotionEvents.Messages[vectorIndex].Time);

        public TimeSpan GetLastMotionTime() => TimeSpan.FromTicks(MotionEvents.Messages[MotionEvents.Messages.Count - 1].Time);
    }
}