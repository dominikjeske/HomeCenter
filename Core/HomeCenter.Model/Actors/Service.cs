using HomeCenter.Model.Core;
using System.Collections.Generic;

namespace HomeCenter.Model.Actors
{
    public abstract class Service : DeviceActor
    {
        [Map] protected List<AttachedProperty> ComponentsAttachedProperties { get; init; }
        [Map] protected List<AttachedProperty> AreasAttachedProperties { get; init; }
    }
}