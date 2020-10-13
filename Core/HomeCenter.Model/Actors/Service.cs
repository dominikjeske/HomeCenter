using HomeCenter.Model.Core;
using System.Collections.Generic;

namespace HomeCenter.Model.Actors
{
    public abstract class Service : DeviceActor
    {
        protected List<AttachedProperty> ComponentsAttachedProperties { get; init; }
        protected List<AttachedProperty> AreasAttachedProperties { get; init; }
    }
}