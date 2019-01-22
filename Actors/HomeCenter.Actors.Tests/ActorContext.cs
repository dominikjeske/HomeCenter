using HomeCenter.Model.Messages.Commands;
using HomeCenter.Model.Messages.Queries;
using HomeCenter.Model.Messages.Queries.Services;
using Proto;
using System.Threading.Tasks;

namespace HomeCenter.Services.MotionService.Tests
{
    internal class ActorContext
    {
        public RootContext Context { get; } = new RootContext();
        public PID PID { get; set; }

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
        /// Allows to wait for actor to process pevious task
        /// </summary>
        /// <returns></returns>
        public bool IsAlive()
        {
            return Context.RequestAsync<bool>(new PID("nonhost", "motionService"), IsAliveQuery.Default).GetAwaiter().GetResult();
        }
    }
}