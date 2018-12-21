using HomeCenter.Model.Adapters;
using HomeCenter.Model.Capabilities;
using HomeCenter.Model.Messages;
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


        // Byte 0 = Offset
        // Register 0-1=Input
        // Register 2-3=Output
        // Register 4-5=Inversion
        // Register 6-7=Configuration
        // Register 8=Timeout

        //byte 0 = Set target register to OUTPUT-1 register
        //byte 1 = The state of ports 0-7
        //byte 2 = The state of ports 8-15
        private readonly byte[] _outputWriteBuffer = { 2, 0, 0 };

        //byte 0 = Set target register to CONFIGURATION register
        //byte 1 = Set CONFIGURATION-1 to outputs
        //byte 2 = Set CONFIGURATION-2 to outputs
        private readonly byte[] _configurationWriteBuffer = { 6, 0, 0 };


        private int _poolDurationWarning;
        protected int _i2cAddress;
        protected TimeSpan _poolInterval;

        private byte[] _committedState;
        private byte[] _state;

        protected CCToolsBaseAdapter()
        {
            _requierdProperties.Add(MessageProperties.PinNumber);
        }

        protected override async Task OnStarted(IContext context)
        {
            await base.OnStarted(context).ConfigureAwait(false);

            _poolInterval = AsIntTime(MessageProperties.PoolInterval);
            _poolDurationWarning = AsInt(MessageProperties.PollDurationWarningThreshold);
            _i2cAddress = AsInt(MessageProperties.Address);

            _state = new byte[StateSize];
            _committedState = new byte[StateSize];
        }

        protected Task Refresh(RefreshCommand message) => FetchState();

        protected bool QueryState(StateQuery message)
        {
            var pinNumber = AsInt(MessageProperties.PinNumber);
            return GetPortState(pinNumber);
        }

        protected Task SetPortState(int pinNumber, bool state)
        {
            _state.SetBit(pinNumber, state);

            return CommitChanges();
        }

        protected Task SetState(byte[] state)
        {
            if (state == null) throw new ArgumentNullException(nameof(state));

            Buffer.BlockCopy(state, 0, _state, 0, state.Length);

            return CommitChanges();
            
        }

        private async Task FetchState()
        {
            var stopwatch = Stopwatch.StartNew();

            var newState = await ReadFromBus().ConfigureAwait(false);

            stopwatch.Stop();

            if (newState.SequenceEqual(_state) || newState.Length == 0) return;

            var oldState = _state.ToArray();

            Buffer.BlockCopy(newState, 0, _state, 0, newState.Length);
            Buffer.BlockCopy(newState, 0, _committedState, 0, newState.Length);

            var oldStateBits = new BitArray(oldState);
            var newStateBits = new BitArray(newState);

            Logger.LogInformation($"'{Uid}' fetched different state ({oldState.ToBinaryString()}->{newState.ToBinaryString()})");

            for (int i = 0; i < oldStateBits.Length; i++)
            {
                var oldPinState = oldStateBits.Get(i);
                var newPinState = newStateBits.Get(i);
                
                if (oldPinState == newPinState) continue;

                var properyChangeEvent = PropertyChangedEvent.Create(Uid, PowerState.StateName, oldPinState, newPinState, new Dictionary<string, string>()
                {
                    [MessageProperties.PinNumber] = i.ToString()
                });

                await MessageBroker.PublisEvent(properyChangeEvent, _requierdProperties).ConfigureAwait(false);
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

            Logger.LogInformation("Board '" + Uid + "' committed state '" + BitConverter.ToString(_state) + "'.");
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
            var query = I2cQuery.Create(_i2cAddress, new byte[] { 0 });
            var result = await MessageBroker.QueryService<I2cQuery, byte[]>(query).ConfigureAwait(false);
            return result;
        }
    }
}