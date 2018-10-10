using HomeCenter.Model.Adapters;
using HomeCenter.Model.Capabilities;
using HomeCenter.Model.Capabilities.Constants;
using HomeCenter.Model.Core;
using HomeCenter.Model.Extensions;
using HomeCenter.Model.Messages.Commands.Device;
using HomeCenter.Model.Messages.Events.Device;
using HomeCenter.Model.Messages.Queries.Device;
using HomeCenter.Model.Native;
using HomeCenter.Model.ValueTypes;
using HomeCenter.Utils.Extensions;
using Microsoft.Extensions.Logging;
using Proto;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace HomeCenter.Adapters.Common
{
    public abstract class CCToolsBaseAdapter : Adapter
    {
        private int _poolDurationWarning;
        private byte[] _committedState;
        private byte[] _state;

        protected readonly II2CBusService _i2CBusService;
        protected II2CPortExpanderDriver _portExpanderDriver;

        protected CCToolsBaseAdapter(IAdapterServiceFactory adapterServiceFactory) : base(adapterServiceFactory)
        {
            _i2CBusService = adapterServiceFactory.GetI2CService();

            _requierdProperties.Add(AdapterProperties.PinNumber);
        }

        protected override async Task OnStarted(IContext context)
        {
            await base.OnStarted(context).ConfigureAwait(false);

            var poolInterval = this[AdapterProperties.PoolInterval].AsIntTimeSpan();

            _poolDurationWarning = (IntValue)this[AdapterProperties.PollDurationWarningThreshold];

            _state = new byte[_portExpanderDriver.StateSize];
            _committedState = new byte[_portExpanderDriver.StateSize];

            await ScheduleDeviceRefresh<RefreshStateJob>(poolInterval).ConfigureAwait(false);
        }

        protected Task Refresh(RefreshCommand message) => FetchState();

        protected void UpdateState(UpdateStateCommand message)
        {
            var state = message[PowerState.StateName] as StringValue;
            var pinNumber = message[AdapterProperties.PinNumber] as IntValue;
            SetPortState(pinNumber.Value, PowerStateValue.ToBinaryState(state), true);
        }

        protected DiscoveryResponse QueryCapabilities(DiscoverQuery message)
        {
            return new DiscoveryResponse(RequierdProperties(), new PowerState());
        }

        protected bool QueryState(StateQuery message)
        {
            var state = message[PowerState.StateName] as StringValue;
            var pinNumber = message[AdapterProperties.PinNumber] as IntValue;
            return GetPortState(pinNumber);
        }

        protected void SetState(byte[] state, bool commit)
        {
            if (state == null) throw new ArgumentNullException(nameof(state));

            Buffer.BlockCopy(state, 0, _state, 0, state.Length);

            if (commit) CommitChanges();
        }

        private async Task FetchState()
        {
            var stopwatch = Stopwatch.StartNew();

            var newState = _portExpanderDriver.Read();

            stopwatch.Stop();

            if (newState.SequenceEqual(_state)) return;

            var oldState = _state.ToArray();

            Buffer.BlockCopy(newState, 0, _state, 0, newState.Length);
            Buffer.BlockCopy(newState, 0, _committedState, 0, newState.Length);

            var oldStateBits = new BitArray(oldState);
            var newStateBits = new BitArray(newState);

            for (int i = 0; i < oldStateBits.Length; i++)
            {
                var oldPinState = oldStateBits.Get(i);
                var newPinState = newStateBits.Get(i);

                if (oldPinState == newPinState) return;

                var properyChangeEvent = new PropertyChangedEvent(Uid, PowerState.StateName, new BooleanValue(oldPinState),
                                            new BooleanValue(newPinState), new Dictionary<string, IValue>() { { AdapterProperties.PinNumber, new IntValue(i) } });

                await PublisEvent(properyChangeEvent, _requierdProperties).ConfigureAwait(false);

                _logger.LogInformation($"'{Uid}' fetched different state ({oldState.ToBitString()}->{newState.ToBitString()})");
            }

            if (stopwatch.ElapsedMilliseconds > _poolDurationWarning)
            {
                _logger.LogWarning($"Polling device '{Uid}' took {stopwatch.ElapsedMilliseconds} ms.");
            }
        }

        private void CommitChanges(bool force = false)
        {
            if (!force && _state.SequenceEqual(_committedState)) return;

            _portExpanderDriver.Write(_state);
            Buffer.BlockCopy(_state, 0, _committedState, 0, _state.Length);

            _logger.LogWarning("Board '" + Uid + "' committed state '" + BitConverter.ToString(_state) + "'.");
        }

        private bool GetPortState(int id) => _state.GetBit(id);

        private void SetPortState(int pinNumber, bool state, bool commit)
        {
            _state.SetBit(pinNumber, state);

            if (commit) CommitChanges();
        }
    }
}