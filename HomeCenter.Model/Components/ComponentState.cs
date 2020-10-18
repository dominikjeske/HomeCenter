using CSharpFunctionalExtensions;
using HomeCenter.Model.Capabilities;
using HomeCenter.Model.Capabilities.Constants;
using HomeCenter.Model.Messages.Commands;
using HomeCenter.Model.Messages.Queries.Device;
using HomeCenter.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace HomeCenter.Model.Components
{
    public class ComponentState
    {
        private readonly Dictionary<string, State> _states = new Dictionary<string, State>();

        public ComponentState(Dictionary<AdapterReference, DiscoveryResponse> adapterConfiguration)
        {
            var stateMap = new List<(AdapterReference Adapter, StateBase State)>();
            foreach (var adapter in adapterConfiguration.Keys)
            {
                foreach (var state in adapterConfiguration[adapter].SupportedStates)
                {
                    stateMap.Add((adapter, state));
                }
            }

            foreach (var stateGroup in stateMap.GroupBy(x => x.State.Name))
            {
                var state = new State(stateGroup.Key, stateGroup);

                _states.Add(state.Name, state);
            }
        }

        public IReadOnlyList<string> SupportedStates() => _states.Keys.ToList().AsReadOnly();

        public IReadOnlyList<string> SupportedCapabilities() => _states.Values.Select(c => c.Capability).Distinct().ToList().AsReadOnly();

        public bool IsStateProvidingAdapter(string adapterName, string stateName) => _states.ContainsKey(stateName) && (_states[stateName].ReadAdapter?.InvariantEquals(adapterName) ?? false);

        public IEnumerable<string> GetCommandAdapter(Command command) => _states.Where(s => s.Value.SupportedCommands.InvariantContains(command.Type)).Select(c => c.Value.WriteAdapter);

        public IReadOnlyDictionary<string, string> GetStateValues(params string[] stateNames)
        {
            IEnumerable<State> states = _states.Values;

            if (stateNames.Length > 0)
            {
                states = states.Where(s => stateNames.InvariantContains(s.Name));
            }

            return states.ToDictionary(k => k.Name, v => v.Value).AsReadOnly();
        }

        public bool TryUpdateState(string stateName, string newValue, out string oldValue)
        {
            if (!_states.ContainsKey(stateName))
            {
                oldValue = null;
                return false;
            }

            oldValue = _states[stateName].Value;

            if (newValue.Equals(oldValue)) return false;

            _states[stateName].UpdateState(newValue);

            return true;
        }

        internal class State
        {
            public string Name { get; }
            public string Capability { get; private set; }
            public string Value { get; private set; }

            public string WriteAdapter { get; private set; }
            public string ReadAdapter { get; private set; }

            public IList<string> SupportedCommands { get; private set; }

            public State(string name, IEnumerable<(AdapterReference Adapter, StateBase State)> stateGroup)
            {
                Name = name;
                SetStateAdapter(stateGroup, ReadWriteMode.Read);
                SetStateAdapter(stateGroup, ReadWriteMode.Write);
            }

            public void UpdateState(string newValue)
            {
                Value = newValue;
            }

            private void SetStateAdapter(string adapter, StateBase state, string mode)
            {
                if (mode == ReadWriteMode.Read)
                {
                    ReadAdapter = adapter;
                    Capability = state.CapabilityName;
                }
                else
                {
                    WriteAdapter = adapter;
                    SupportedCommands = state.SupportedCommands;
                }
            }

            private void SetStateAdapter(IEnumerable<(AdapterReference Adapter, StateBase State)> stateGroup, string readWriteMode)
            {
                var forcedState = stateGroup.FirstOrDefault(a => a.Adapter.ContainsProperty(Name) && a.Adapter[Name] == readWriteMode);

                if (!forcedState.Equals(default))
                {
                    SetStateAdapter(forcedState.Adapter.Uid, forcedState.State, readWriteMode);
                }
                else
                {
                    var readStateAdapters = stateGroup.Where(x => x.State.ReadWrite == readWriteMode || x.State.ReadWrite == ReadWriteMode.ReadWrite);
                    var adaptersCount = readStateAdapters.Count();

                    if (adaptersCount == 0) return;
                    if (adaptersCount > 1)
                    {
                        throw new ArgumentException($"Component have more than one adapter that can provide state '{Name}': {string.Join(", ", readStateAdapters.Select(a => a.Adapter.Uid))}");
                    }

                    var readStateAdapter = readStateAdapters.First();
                    var adapter = readStateAdapter.Adapter.Uid;
                    var st = stateGroup.Single(a => a.Adapter.Uid == adapter).State;

                    SetStateAdapter(adapter, st, readWriteMode);
                }
            }
        }
    }
}