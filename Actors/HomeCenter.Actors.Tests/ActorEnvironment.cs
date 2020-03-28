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
        public RootContext Context { get; } = new RootContext();
        public PID PID { get; set; }

        public TestScheduler Scheduler { get; set; }

        public FakeLogger<LightAutomationServiceProxy> Logger { get; set; }

        public ITestableObservable<MotionEnvelope> MotionEvents { get; set; }

        public Dictionary<string, FakeMotionLamp> Lamps { get; set; }

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