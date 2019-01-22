using HomeCenter.Model.Core;
using HomeCenter.Model.Messages.Queries.Services;
using System.Collections.Generic;

namespace HomeCenter.Model.Actors
{
    public abstract class Service : DeviceActor
    {
        [Map] protected List<AttachedProperty> ComponentsAttachedProperties { get; private set; }
        [Map] protected List<AttachedProperty> AreasAttachedProperties { get; private set; }

        
    }
}