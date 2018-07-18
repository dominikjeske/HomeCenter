using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using HomeCenter.ComponentModel.Adapters.Drivers;
using HomeCenter.ComponentModel.Capabilities;
using HomeCenter.ComponentModel.Capabilities.Constants;
using HomeCenter.ComponentModel.Commands;
using HomeCenter.ComponentModel.Commands.Responses;
using HomeCenter.ComponentModel.Events;
using HomeCenter.ComponentModel.ValueTypes;
using HomeCenter.Core.EventAggregator;
using HomeCenter.Core.Extensions;
using HomeCenter.Core.Services.I2C;
using HomeCenter.Core.Services.Logging;
using HomeCenter.Model.Extensions;

namespace HomeCenter.ComponentModel.Adapters
{
    public abstract class CCToolsBaseAdapter : Adapter
    {
        private int _poolDurationWarning;
        private byte[] _committedState;
        private byte[] _state;

        protected readonly ILogger _log;
        protected readonly II2CBusService _i2CBusService;
        protected II2CPortExpanderDriver _portExpanderDriver;

        protected CCToolsBaseAdapter(IAdapterServiceFactory adapterServiceFactory) : base(adapterServiceFactory)
        {
            _i2CBusService = adapterServiceFactory.GetI2CService();
            _log = adapterServiceFactory.GetLogger().CreatePublisher($"{nameof(CCToolsBaseAdapter)}_{Uid}");

            _requierdProperties.Add(AdapterProperties.PinNumber);
        }

        public override async Task Initialize()
        {
            var poolInterval = this[AdapterProperties.PoolInterval].AsIntTimeSpan();

            _poolDurationWarning = (IntValue)this[AdapterProperties.PollDurationWarningThreshold];

            _state = new byte[_portExpanderDriver.StateSize];
            _committedState = new byte[_portExpanderDriver.StateSize];

            await ScheduleDeviceRefresh<RefreshStateJob>(poolInterval).ConfigureAwait(false);

            _disposables.Add(_eventAggregator.SubscribeForDeviceQuery<DeviceCommand>(DeviceCommandHandler, Uid));

            await base.Initialize().ConfigureAwait(false);
        }

        private Task<object> DeviceCommandHandler(IMessageEnvelope<DeviceCommand> messageEnvelope)
        {
            return ExecuteCommand(messageEnvelope.Message);
        }

        protected Task RefreshCommandHandler(Command message) => FetchState();

        protected DiscoveryResponse DiscoverCapabilitiesHandler(Command message)
        {
            return new DiscoveryResponse(RequierdProperties(), new PowerState());
        }

        protected void UpdateCommandHandler(Command message)
        {
            var state = message[PowerState.StateName] as StringValue;
            var pinNumber = message[AdapterProperties.PinNumber] as IntValue;
            SetPortState(pinNumber.Value, PowerStateValue.ToBinaryState(state), true);
        }

        protected bool QueryCommandHandler(Command message)
        {
            var state = message[PowerState.StateName] as StringValue;
            var pinNumber = message[AdapterProperties.PinNumber] as IntValue;
            return GetPortState(pinNumber);
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

                await _eventAggregator.PublishDeviceEvent(properyChangeEvent, _requierdProperties).ConfigureAwait(false);

                _log.Info($"'{Uid}' fetched different state ({oldState.ToBitString()}->{newState.ToBitString()})");
            }

            if (stopwatch.ElapsedMilliseconds > _poolDurationWarning)
            {
                _log.Warning($"Polling device '{Uid}' took {stopwatch.ElapsedMilliseconds} ms.");
            }
        }

        protected void SetState(byte[] state, bool commit)
        {
            if (state == null) throw new ArgumentNullException(nameof(state));

            Buffer.BlockCopy(state, 0, _state, 0, state.Length);

            if (commit) CommitChanges();
        }

        private void CommitChanges(bool force = false)
        {
            if (!force && _state.SequenceEqual(_committedState)) return;

            _portExpanderDriver.Write(_state);
            Buffer.BlockCopy(_state, 0, _committedState, 0, _state.Length);

            _log.Verbose("Board '" + Uid + "' committed state '" + BitConverter.ToString(_state) + "'.");
        }

        private bool GetPortState(int id) => _state.GetBit(id);

        private void SetPortState(int pinNumber, bool state, bool commit)
        {
            _state.SetBit(pinNumber, state);

            if (commit) CommitChanges();
        }
    }
}