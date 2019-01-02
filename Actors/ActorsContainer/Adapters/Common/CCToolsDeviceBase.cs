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
using System.Threading.Tasks;

namespace HomeCenter.Adapters.Common
{
    public abstract class CCToolsBaseAdapter : Adapter
    {
        protected readonly MAX7311Driver _driver = new MAX7311Driver();

        private int _poolDurationWarning;
        private int _i2cAddress;

        protected Task ConfigureDriver(bool firstPortWriteMode, bool secondPortWriteMode)
        {
            return MessageBroker.SendToService(I2cCommand.Create(_i2cAddress, _driver.Configure(firstPortWriteMode, secondPortWriteMode)));
        }

        protected CCToolsBaseAdapter()
        {
            _requierdProperties.Add(MessageProperties.PinNumber);
        }

        protected override async Task OnStarted(IContext context)
        {
            await base.OnStarted(context).ConfigureAwait(false);

            _poolDurationWarning = AsInt(MessageProperties.PollDurationWarningThreshold, 2000);
            _i2cAddress = AsInt(MessageProperties.Address);
        }

        protected Task Refresh(RefreshCommand message) => FetchState();

        protected bool QueryState(StateQuery message)
        {
            var pinNumber = AsInt(MessageProperties.PinNumber);
            return _driver.GetState(pinNumber);
        }

        protected async Task SetPortState(int pinNumber, bool state)
        {
            var newState = _driver.GenerateNewState(pinNumber, state);

            try
            {
                await MessageBroker.SendToService(I2cCommand.Create(_i2cAddress, newState)).ConfigureAwait(false);
                _driver.AcceptNewState();
            }
            catch (Exception)
            {
                _driver.RevertNewState();
                throw;
            }

            Logger.LogInformation("Board '" + Uid + "' committed state '" + _driver.GetState().ToBinaryString() + "'.");
        }

        protected async Task FetchState()
        {
            var stopwatch = Stopwatch.StartNew();

            var newState = await ReadFromBus().ConfigureAwait(false);

            stopwatch.Stop();

            if (!_driver.TrySaveState(newState, out var oldState)) return;

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

                await MessageBroker.PublishEvent(properyChangeEvent).ConfigureAwait(false);
            }

            if (stopwatch.ElapsedMilliseconds > _poolDurationWarning)
            {
                Logger.LogWarning($"Polling device '{Uid}' took {stopwatch.ElapsedMilliseconds} ms.");
            }
        }

        private async Task<byte[]> ReadFromBus()
        {
            var query = I2cQuery.Create(_i2cAddress, _driver.GetReadTable(), _driver.BufferSize);
            var result = await MessageBroker.QueryService<I2cQuery, byte[]>(query).ConfigureAwait(false);
            return result;
        }
    }
}