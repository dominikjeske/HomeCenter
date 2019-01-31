using HomeCenter.Model.Core;
using HomeCenter.Model.Messages.Commands;
using HomeCenter.Model.Messages.Events;
using HomeCenter.Model.Messages.Queries;

namespace HomeCenter.Model.Messages
{
    public interface IMessageFactory
    {
        Command CreateCommand(string message);
        Command CreateCommand(string type, BaseObject source);
        Event CreateEvent(string message);
        Event CreateEvent(string type, BaseObject source);
        Query CreateQuery(string message);
        Query CreateQuery(string type, BaseObject source);
        T TransformMessage<T>(BaseObject source, BaseObject target) where T : ActorMessage;
    }
}