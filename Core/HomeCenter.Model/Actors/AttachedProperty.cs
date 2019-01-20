using HomeCenter.Model.Core;

namespace HomeCenter.Model.Actors
{
    public class AttachedProperty : BaseObject
    {
        public string Service { get; set; }

        public string AttachedActor { get; set; }

        public string AttachedArea { get; set; }
    }
}