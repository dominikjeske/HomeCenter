using HomeCenter.Model.Actors;
using HomeCenter.Model.Components;
using HomeCenter.Model.Core;
using System.Collections.Generic;

namespace HomeCenter.Model.Areas
{
    public class Area : DeviceActor
    {
        [Map] private List<Area> _areas { get; set; } = new List<Area>();

        [Map] private IList<Component> _components { get; set; } = new List<Component>();

        public IReadOnlyCollection<Area> Areas => _areas.AsReadOnly();

        public override string ToString() => $"{Uid} with {_components.Count} components";
    }
}