using HomeCenter.Abstractions;

namespace HomeCenter.Model.Components
{
    public class Translator : BaseObject
    {
        public ActorMessage From { get; set; }
        public ActorMessage To { get; set; }
    }
}