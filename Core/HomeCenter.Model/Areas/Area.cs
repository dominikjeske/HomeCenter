using HomeCenter.Model.Core;
using Proto;
using System.Collections.Generic;

namespace HomeCenter.Model.Areas
{
    public class Area : BaseObject
    {
        [Map] private List<Area> _areas { get; set; } = new List<Area>();
        private IDictionary<string, PID> _components = new Dictionary<string, PID>();

        public IReadOnlyCollection<Area> Areas => _areas.AsReadOnly();

        public void AddComponent(string uid, PID component)
        {
            _components.Add(uid, component);
        }

        public override string ToString() => $"{Uid} with {_components.Count} components";
    }
}