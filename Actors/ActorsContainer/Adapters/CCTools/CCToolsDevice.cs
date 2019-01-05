using HomeCenter.CodeGeneration;
using HomeCenter.Model.Adapters;
using HomeCenter.Model.Capabilities;
using HomeCenter.Model.Exceptions;
using HomeCenter.Model.Messages;
using HomeCenter.Model.Messages.Commands;
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
    [ProxyCodeGenerator]
    public abstract class CCToolsAdapter : Adapter
    {
        private readonly MAX7311Driver _driver = new MAX7311Driver();

        private int _poolDurationWarning;
        private int _i2cAddress;
        private bool _firstPortWriteMode;
        private bool _secondPortWriteMode;

        protected CCToolsAdapter()
        {
            _requierdProperties.Add(MessageProperties.PinNumber);
        }

        protected override async Task OnStarted(IContext context)
        {
            await base.OnStarted(context).ConfigureAwait(false);

            _poolDurationWarning = AsInt(MessageProperties.PollDurationWarningThreshold, 2000);
            _i2cAddress = AsInt(MessageProperties.Address);
            _firstPortWriteMode = AsBool(MessageProperties.FirstPortWriteMode);
            _secondPortWriteMode = AsBool(MessageProperties.SecondPortWriteMode);

            if (ContainsProperty(MessageProperties.InterruptPin) && ContainsProperty(MessageProperties.InterruptSource))
            {
                Subscribe<PinValueChangedEvent>(new Broker.RoutingFilter(new Dictionary<string, string>()
                {
                    [MessageProperties.MessageSource] = AsString(MessageProperties.InterruptSource),
                    [MessageProperties.PinNumber] = AsString(MessageProperties.InterruptPin)
                }));
            }

            await ConfigureDriver().ConfigureAwait(false);
            await FetchState().ConfigureAwait(false);
        }
        
        private Task ConfigureDriver()
        {
            return MessageBroker.SendToService(I2cCommand.Create(_i2cAddress, _driver.Configure(_firstPortWriteMode, _secondPortWriteMode)));
        }

        protected Task Hadle(PinValueChangedEvent pinValueChangedEvent) => FetchState();

        protected Task Handle(RefreshCommand message) => FetchState();
        
        protected DiscoveryResponse Handle(DiscoverQuery message)
        {
            return new DiscoveryResponse(RequierdProperties(), new PowerState());
        }

        protected Task Handle(TurnOnCommand message)
        {
            int pinNumber = ValidatePin(message);

            return SetPortState(pinNumber, true);
        }

        protected Task Handle(TurnOffCommand message)
        {
            int pinNumber = ValidatePin(message);

            return SetPortState(pinNumber, false);
        }

        protected Task Handle(SwitchPowerStateCommand message)
        {
            int pinNumber = ValidatePin(message);
            var currentState = _driver.GetState(pinNumber);

            return SetPortState(pinNumber, !currentState);
        }
        
        protected bool QueryState(StateQuery message)
        {
            var pinNumber = AsInt(MessageProperties.PinNumber);
            return _driver.GetState(pinNumber);
        }
        
        private int ValidatePin(Command message)
        {
            var pinNumber = message.AsInt(MessageProperties.PinNumber);
            if (pinNumber < 0 || pinNumber > 15) throw new ArgumentOutOfRangeException(nameof(pinNumber));
            var isPinInFirstPortRange = pinNumber < 8;

            if(isPinInFirstPortRange && !_firstPortWriteMode || !isPinInFirstPortRange && !_secondPortWriteMode)
            {
                throw new UnsupportedStateException($"Pin {pinNumber} on device {Uid} is configured for INPUT");
            }
            
            return pinNumber;
        }
        
        private async Task SetPortState(int pinNumber, bool state)
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

        private async Task FetchState()
        {
            var stopwatch = Stopwatch.StartNew();

            var newState = await ReadFromBus().ConfigureAwait(false);

            stopwatch.Stop();

            if (!_driver.TrySaveState(newState, out var oldState)) return;

            var oldStateBits = new BitArray(oldState);
            var newStateBits = new BitArray(newState);

            Logger.LogInformation($"[{Uid}] fetched different state ({oldState.ToBinaryString()}->{newState.ToBinaryString()})");

            for (int pinNumber = 0; pinNumber < oldStateBits.Length; pinNumber++)
            {
                var oldPinState = oldStateBits.Get(pinNumber);
                var newPinState = newStateBits.Get(pinNumber);
                var isPinInFirstPortRange = pinNumber < 8;

                // When state is the same or change is in port that are set to WRITE we skip event generation
                if (oldPinState == newPinState || (isPinInFirstPortRange && _firstPortWriteMode) || (!isPinInFirstPortRange && _secondPortWriteMode)) continue;
                
                var properyChangeEvent = PropertyChangedEvent.Create(Uid, PowerState.StateName, oldPinState, newPinState, new Dictionary<string, string>()
                {
                    [MessageProperties.PinNumber] = pinNumber.ToString()
                });

                await MessageBroker.PublishEvent(properyChangeEvent, Uid).ConfigureAwait(false);

                Logger.LogInformation($"[{Uid}] port state '{pinNumber}' changed {oldPinState}->{newPinState}");
            }

            if (stopwatch.ElapsedMilliseconds > _poolDurationWarning)
            {
                Logger.LogWarning($"Polling device '{Uid}' took {stopwatch.ElapsedMilliseconds}ms.");
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