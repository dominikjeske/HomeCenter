using HomeCenter.Abstractions;

namespace HomeCenter.Model.Components
{
    public class Translator : BaseObject
    {
        public ActorMessage From { get; }
        public ActorMessage To { get; }

        public Translator(ActorMessage from, ActorMessage to)
        {
            From = from;
            To = to;
        }
    }
}