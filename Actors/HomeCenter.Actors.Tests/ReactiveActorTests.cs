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
        protected void AdvanceTo(long ticks) => _context.Scheduler.AdvanceTo(ticks);
        protected void AdvanceJustAfterEnd() => _context.Scheduler.AdvanceJustAfterEnd(_context.MotionEvents);
        protected void AdvanceJustAfter(TimeSpan time) => _context.Scheduler.AdvanceJustAfter(time);
        protected bool LampState(string lamp) => _context.Lamps[lamp].IsTurnedOn;
        protected void SendCommand(Command command) => _context.Send(command);
        protected Task<T> Query<T>(Query query) => _context.Query<T>(query);
        protected void RunAfterFirstMove(Action<MotionEnvelope> action) => _context.MotionEvents.Subscribe(action);
    }
}