using Proto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace HomeCenter.Abstractions
{
    public sealed class ActorMessageContext
    {
        public static ActorMessageContext Create(PID actor, Command command)
        {
            return new ActorMessageContext(actor)
            {
                Commands = new List<Command>(new Command[] { command })
            };
        }

        public static ActorMessageContext Create(PID actor, IValidable condition, IEnumerable<Command> commands)
        {
            return new ActorMessageContext(actor) 
            { 
                Condition = condition,
                Commands = new List<Command>(commands)
            };
        }

        public static ActorMessageContext Create(PID actor, IValidable condition, IEnumerable<Command> finishCommands, TimeSpan finishComandTime, IEnumerable<Command> commands)
        {
            return new ActorMessageContext(actor)
            {
                Condition = condition,
                FinishCommandTime = finishComandTime,
                FinishCommands = new List<Command>(finishCommands),
                Commands = new List<Command>(commands)
            };
        }

        public string GetMessageUid()
        {
            return $"{Actor.Id}-{string.Join("-", Commands.Select(c => c.Type))}";
        }

        private ActorMessageContext(PID actor)
        {
            Actor = actor;
        }

        public IValidable Condition { get; init; } = EmptyCondition.Default;
        public PID Actor { get; }
        public IEnumerable<Command> Commands { get; init; } = new List<Command>();
        public IEnumerable<Command> FinishCommands { get; init; } = new List<Command>();
        public TimeSpan? FinishCommandTime { get; init; }
        public CancellationToken Token { get; }
    }
}