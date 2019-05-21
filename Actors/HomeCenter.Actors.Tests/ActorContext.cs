using HomeCenter.Model.Messages.Commands;
using HomeCenter.Model.Messages.Queries;
using HomeCenter.Model.Messages.Queries.Services;
using Microsoft.Reactive.Testing;
using Proto;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HomeCenter.Services.MotionService.Tests
{
    internal class ActorContext
    {
        public RootContext Context { get; } = new RootContext();
        public PID PID { get; set; }

        public TestScheduler Scheduler { get; set; }

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
    }
}