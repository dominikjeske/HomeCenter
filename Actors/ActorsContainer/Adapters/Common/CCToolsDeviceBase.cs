using HomeCenter.Model.Adapters;
using HomeCenter.Model.Capabilities;
using HomeCenter.Model.Messages.Commands.Device;
using HomeCenter.Model.Messages.Commands.Service;
using HomeCenter.Model.Messages.Events.Device;
using HomeCenter.Model.Messages.Queries.Device;
using HomeCenter.Model.Messages.Queries.Services;
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
        private const int StateSize = 2;
        private readonly byte[] _outputWriteBuffer = { 2, 0, 0 };
        private readonly byte[] _configurationWriteBuffer = { 6, 0, 0 };

        private int _poolDurationWarning;
        protected int _i2cAddress;
        protected TimeSpan _poolInterval;

        private byte[] _committedState;
        private byte[] _state;

        protected CCToolsBaseAdapter()
        {
            _requierdProperties.Add(AdapterProperties.PinNumber);
        }

        protected override async Task OnStarted(IContext context)
        {
            await base.OnStarted(context).ConfigureAwait(false);

            _poolInterval = AsIntTime(AdapterProperties.PoolInterval);
            _poolDurationWarning = AsInt(AdapterProperties.PollDurationWarningThreshold);
            _i2cAddress = AsInt(AdapterProperties.I2cAddress);

            _state = new byte[StateSize];
            _committedState = new byte[StateSize];
        }

        protected Task Refresh(RefreshCommand message) => FetchState();

        protected bool QueryState(StateQuery message)
        {
            var pinNumber = AsInt(AdapterProperties.PinNumber);
            return GetPortState(pinNumber);
        }

        protected async Task SetPortState(int pinNumber, bool state, bool commit)
        {
            _state.SetBit(pinNumber, state);

            if (commit)
            {
                await CommitChanges().ConfigureAwait(false);
            }
        }

        protected async Task SetState(byte[] state, bool commit)
        {
            if (state == null) throw new ArgumentNullException(nameof(state));

            Buffer.BlockCopy(state, 0, _state, 0, state.Length);

            if (commit)
            {
                await CommitChanges().ConfigureAwait(false);
            }
        }

        private async Task FetchState()
        {
            var stopwatch = Stopwatch.StartNew();

            var newState = await ReadFromBus().ConfigureAwait(false);

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

                if (oldPinState == newPinState) continue;

                var properyChangeEvent = new PropertyChangedEvent(Uid, PowerState.StateName, oldPinState, newPinState, new Dictionary<string, string>() { [AdapterProperties.PinNumber] = i.ToString() });

                await MessageBroker.PublisEvent(properyChangeEvent, _requierdProperties).ConfigureAwait(false);

                Logger.LogInformation($"'{Uid}' fetched different state ({oldState.ToBitString()}->{newState.ToBitString()})");
            }

            if (stopwatch.ElapsedMilliseconds > _poolDurationWarning)
            {
                Logger.LogWarning($"Polling device '{Uid}' took {stopwatch.ElapsedMilliseconds} ms.");
            }
        }

        private async Task CommitChanges(bool force = false)
        {
            if (!force && _state.SequenceEqual(_committedState)) return;

            await WriteToBus(_state).ConfigureAwait(false);
            Buffer.BlockCopy(_state, 0, _committedState, 0, _state.Length);

            Logger.LogWarning("Board '" + Uid + "' committed state '" + BitConverter.ToString(_state) + "'.");
        }

        private bool GetPortState(int id) => _state.GetBit(id);

        private async Task WriteToBus(byte[] state)
        {
            if (state == null) throw new ArgumentNullException(nameof(state));
            if (state.Length != StateSize) throw new ArgumentException("Length is invalid.", nameof(state));

            // Set configuration to output.
            var cmd = I2cCommand.Create(_i2cAddress, _configurationWriteBuffer);
            await MessageBroker.SendToService(cmd).ConfigureAwait(false);

            // Update the output registers only.
            _outputWriteBuffer[1] = state[0];
            _outputWriteBuffer[2] = state[1];

            cmd = I2cCommand.Create(_i2cAddress, _outputWriteBuffer);
            await MessageBroker.SendToService(cmd).ConfigureAwait(false);
        }

        private async Task<byte[]> ReadFromBus()
        {
            var query = I2cQuery.Create(_i2cAddress, StateSize);
            var result = await MessageBroker.QueryService<I2cQuery, byte[]>(query).ConfigureAwait(false);
            return result;
        }
    }
}