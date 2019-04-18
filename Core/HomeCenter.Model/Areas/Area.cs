using HomeCenter.CodeGeneration;
using HomeCenter.Model.Actors;
using HomeCenter.Model.Components;
using HomeCenter.Model.Core;
using System.Collections.Generic;

namespace HomeCenter.Model.Areas
{
    [ProxyCodeGenerator]
    public class Area : DeviceActor
    {
        [Map] private List<Area> _areas { get; set; } = new List<Area>();

        [Map] private IList<Component> _components { get; set; } = new List<Component>();

        public override string ToString() => $"{Uid} with {_components.Count} components";
    }
}