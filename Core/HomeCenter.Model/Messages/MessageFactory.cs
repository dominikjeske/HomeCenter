using HomeCenter.CodeGeneration;
using HomeCenter.Model.Core;
using HomeCenter.Model.Messages.Commands;
using HomeCenter.Model.Messages.Events;
using HomeCenter.Model.Messages.Queries;
using System;

namespace HomeCenter.Model.Messages
{
    [CommandBuilder]
    public partial class MessageFactory : IMessageFactory
    {
        public Event CreateEvent(string type, BaseObject source) => CreateEvent(type).SetProperties(source.GetProperties()) as Event;

        public T TransformMessage<T>(BaseObject source, BaseObject target) where T : ActorMessage
        {
            ActorMessage message;
            if (typeof(T) == typeof(Event))
            {
                message = CreateEvent(target.Type, source);
            }
            else if (typeof(T) == typeof(Query))
            {
                message = CreateQuery(target.Type, source);
            }
            else if (typeof(T) == typeof(Command))
            {
                message = CreateCommand(target.Type, source);
            }
            else
            {
                throw new NotSupportedException($"Type {typeof(T).Name} is not supported");
            }

            foreach(var prop in target.GetPropetiesKeys())
            {
                message.SetProperty(prop, target[prop]);
            }

            return message as T;
        }

        public Query CreateQuery(string type, BaseObject source) => CreateQuery(type).SetProperties(source.GetProperties()) as Query;

        public Command CreateCommand(string type, BaseObject source) => CreateEvent(type).SetProperties(source.GetProperties()) as Command;
    }
}