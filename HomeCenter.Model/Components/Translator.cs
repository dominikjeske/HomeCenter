using HomeCenter.Model.Core;
using HomeCenter.Model.Messages;

namespace HomeCenter.Model.Components
{
    public class Translator : BaseObject
    {
        public ActorMessage From { get; set; }
        public ActorMessage To { get; set; }
    }
}