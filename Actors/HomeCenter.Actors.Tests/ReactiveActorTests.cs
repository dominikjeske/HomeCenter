using HomeCenter.Model.Messages.Commands;
using HomeCenter.Model.Messages.Queries;
using Microsoft.Reactive.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading.Tasks;

namespace HomeCenter.Services.MotionService.Tests
{
    public class ReactiveActorTests : ReactiveTest
    {
        internal ActorContext _context;

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

        protected void AdvanceToEnd() => _context.Scheduler.AdvanceToEnd(_context.MotionEvents);

        protected void AdvanceTo(TimeSpan time) => _context.Scheduler.AdvanceTo(time);

        /// <summary>
        /// We calculate next full second after given time and return just after this time (default 100ms)
        /// </summary>
        /// <param name="time"></param>
        protected void AdvanceJustAfterRoundUp(TimeSpan time) => _context.Scheduler.AdvanceJustAfter(TimeSpan.FromSeconds(Math.Ceiling(time.TotalSeconds)));

        protected void AdvanceTo(long ticks) => _context.Scheduler.AdvanceTo(ticks);

        protected void AdvanceJustAfterEnd(int timeAfter = 500) => _context.Scheduler.AdvanceJustAfterEnd(_context.MotionEvents, timeAfter);

        protected void AdvanceJustAfter(TimeSpan time) => _context.Scheduler.AdvanceJustAfter(time);

        protected void AdvanceJustBefore(TimeSpan time) => _context.Scheduler.AdvanceJustBefore(time);

        protected bool LampState(string lamp) => _context.Lamps[lamp].IsTurnedOn;

        protected void SendCommand(Command command) => _context.Send(command);

        protected Task<T> Query<T>(Query query) => _context.Query<T>(query);

        protected void RunAfterFirstMove(Action<MotionEnvelope> action) => _context.MotionEvents.Subscribe(action);

        protected TimeSpan GetMotionTime(int vectorIndex) => TimeSpan.FromTicks(_context.MotionEvents.Messages[vectorIndex].Time);

        protected TimeSpan GetLastMotionTime() => TimeSpan.FromTicks(_context.MotionEvents.Messages[_context.MotionEvents.Messages.Count - 1].Time);
    }
}