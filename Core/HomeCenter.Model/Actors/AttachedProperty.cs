using HomeCenter.Model.Core;

namespace HomeCenter.Model.Actors
{
    public class AttachedProperty : BaseObject
    {
        public string Service { get; set; } = string.Empty;

        public string AttachedActor { get; set; } = string.Empty;

        public string AttachedArea { get; set; } = string.Empty;
    }
}