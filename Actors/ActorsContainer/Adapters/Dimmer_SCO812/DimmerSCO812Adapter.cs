using HomeCenter.CodeGeneration;
using HomeCenter.Model.Adapters;
using HomeCenter.Model.Capabilities;
using HomeCenter.Model.Messages;
using HomeCenter.Model.Messages.Commands;
using HomeCenter.Model.Messages.Commands.Device;
using HomeCenter.Model.Messages.Events.Device;
using HomeCenter.Model.Messages.Events.Service;
using HomeCenter.Model.Messages.Queries.Device;
using HomeCenter.Model.Messages.Scheduler;
using Microsoft.Extensions.Logging;
using Proto;
using System;
using System.Threading.Tasks;

namespace HomeCenter.Adapters.CurrentBridge
{
    [ProxyCodeGenerator]
    public abstract class DimmerSCO812Adapter : Adapter
    {
        private const int CHANGE_POWER_STATE_TIME = 200;
        private const int SWITCH_CHANGE_DIRECTION = 400;
        private const int WAIT_AFTER_CHANGE = 800;

        private string _PowerAdapterUid;
        private int _PowerAdapterPin;
        private string _PowerLevelAdapterUid;
        private int _PowerLevelAdapterPin;
        private double? _Minimum;
        private double? _Maximum;

        private double? _Range;
        private double? _PowerLevel;
        private double? _CurrentValue;
        private double? _PreviousCurrentValue;

        protected override async Task OnStarted(IContext context)
        {
            await base.OnStarted(context).ConfigureAwait(false);

            _PowerAdapterUid = AsString("PowerAdapter");
            _PowerAdapterPin = AsInt("PowerAdapterPin");
            _PowerLevelAdapterUid = AsString("PowerLevelAdapterUid");
            _PowerLevelAdapterPin = AsInt("PowerLevelAdapterPin");
            _Minimum = AsNullableDouble("Minimum");
            _Maximum = AsNullableDouble("Maximum");

            await MessageBroker.Request<DiscoverQuery, DiscoveryResponse>((DiscoverQuery)DiscoverQuery.Default.SetProperty(MessageProperties.PinNumber, _PowerLevelAdapterPin), _PowerLevelAdapterUid).ConfigureAwait(false);

            _disposables.Add(MessageBroker.SubscribeForMessage<PropertyChangedEvent>(Self, _PowerLevelAdapterUid));
        }

        protected override async Task OnSystemStarted(SystemStartedEvent systemStartedEvent)
        {
            await base.OnSystemStarted(systemStartedEvent).ConfigureAwait(false);

            if (!TryCalculateSpectrum())
            {
                Logger.LogInformation($"Calibration of {Uid} : Start");
                Become(CalibrationFirstStateCheck);
                await ChangePowerState();
            }
        }

        private async Task CalibrationFirstStateCheck(IContext context)
        {
            if (context.Message is PropertyChangedEvent property)
            {
                _CurrentValue = property.AsDouble(MessageProperties.NewValue);
                Become(CalibrationSecondStateCheck);
                await ChangePowerState();
            }
            else
            {
                await StandardMode(context).ConfigureAwait(false);
            }
        }

        private async Task CalibrationSecondStateCheck(IContext context)
        {
            if (context.Message is PropertyChangedEvent property)
            {
                var newValue = property.AsDouble(MessageProperties.NewValue);

                // I we changed OFF => ON we have to turn off before start
                if (newValue > _CurrentValue)
                {
                    Logger.LogInformation($"Calibration of {Uid} : detect ON state. Dimmer will be turned off");
                    await ChangePowerState();
                }

                Logger.LogInformation($"Calibration of {Uid} : waiting to reach MAX state");
                Become(CalibrationMaximumLight);
                ForwardToPowerAdapter(TurnOnCommand.Default);
            }
            else
            {
                await StandardMode(context).ConfigureAwait(false);
            }
        }

        private async Task CalibrationMaximumLight(IContext context)
        {
            if (context.Message is StopCommand stopCommand)
            {
                Become(CalibrationMinimumLight);
                ForwardToPowerAdapter(TurnOffCommand.Default);
                await Task.Delay(WAIT_AFTER_CHANGE).ConfigureAwait(false);

                Logger.LogInformation($"Calibration of {Uid} : waiting to reach MIN state");
                ForwardToPowerAdapter(TurnOnCommand.Default);
            }
            else if (context.Message is PropertyChangedEvent maximumState)
            {
                // Resend stop message to cancel scheduled message
                await MessageBroker.SendAfterDelay(ActorMessageContext.Create(Self, StopCommand.Create("MAX")), TimeSpan.FromMilliseconds(1500), true).ConfigureAwait(false);
                _Maximum = maximumState.AsDouble(MessageProperties.NewValue);
            }
            else
            {
                await StandardMode(context).ConfigureAwait(false);
            }
        }

        private async Task CalibrationMinimumLight(IContext context)
        {
            if (context.Message is StopCommand stopCommand)
            {
                // Ignore previous commands if we receive any
                if (stopCommand[MessageProperties.Context] == "MAX")
                {
                    return;
                }

                Become(StandardMode);

                ForwardToPowerAdapter(TurnOffCommand.Default);

                ResetStateValues();

                TryCalculateSpectrum();

                Logger.LogInformation($"Calibration of {Uid} : calibration finished with MIN: {_Minimum}, MAX: {_Maximum}, RANGE: {_Range}");

                await Task.Delay(WAIT_AFTER_CHANGE).ConfigureAwait(false);

                ForwardToPowerAdapter(TurnOnCommand.Create(CHANGE_POWER_STATE_TIME));
            }
            else if (context.Message is PropertyChangedEvent minimumState)
            {
                await MessageBroker.SendAfterDelay(ActorMessageContext.Create(Self, StopCommand.Create("MIN")), TimeSpan.FromMilliseconds(1500)).ConfigureAwait(false);
                _Minimum = minimumState.AsDouble(MessageProperties.NewValue);
            }
            else
            {
                await StandardMode(context).ConfigureAwait(false);
            }
        }

        private void ResetStateValues()
        {
            _CurrentValue = 0;
            _PreviousCurrentValue = 0;
            _PowerLevel = 0;
        }

        protected async Task Handle(PropertyChangedEvent propertyChangedEvent)
        {
            if (!_Minimum.HasValue || !_Range.HasValue) return;

            var value = propertyChangedEvent.AsDouble(MessageProperties.NewValue);
            if (value < _Minimum)
            {
                value = 0;
            }

            _PreviousCurrentValue = _CurrentValue;
            _CurrentValue = value;

            var newLevel = GetPowerLevel(value);

            Logger.LogInformation($"NEW: {newLevel}, MAX: {_Maximum}, MIN: {_Minimum}");

            _PowerLevel = await UpdateState(PowerLevelState.StateName, _PowerLevel, newLevel);
        }

        protected DiscoveryResponse Discover(DiscoverQuery message)
        {
            return new DiscoveryResponse(RequierdProperties(), new PowerLevelState(), new PowerState());
        }

        protected void Handle(TurnOnCommand turnOnCommand)
        {
            if (!turnOnCommand.ContainsProperty(MessageProperties.StateTime))
            {
                // When we don't get state time and we already turned on we ignore this
                // When state time is set by the sender we always allow to pass threw
                if (_PowerLevel > 0) return;

                turnOnCommand.SetProperty(MessageProperties.StateTime, CHANGE_POWER_STATE_TIME);
            }

            ForwardToPowerAdapter(turnOnCommand);
        }

        protected async Task Handle(TurnOffCommand turnOnCommand)
        {
            if (_PowerLevel == 0) return;

            await ChangePowerState();
        }

        protected Task Handle(SetPowerLevelCommand powerLevel)
        {
            var destinationLevel = powerLevel.PowerLevel;

            if (!_PowerLevel.HasValue) return Task.CompletedTask;

            return ControlDimmer(destinationLevel);
        }

        protected async Task Handle(AdjustPowerLevelCommand powerLevel)
        {
            if (!_PowerLevel.HasValue) return;

            var destinationLevel = _PowerLevel.Value + powerLevel.Delta;

            if (destinationLevel > 100)
            {
                destinationLevel = 100.0;
            }
            else if (destinationLevel < 0)
            {
                destinationLevel = 0;
            }

            await ControlDimmer(destinationLevel);
        }

        protected void Handle(CalibrateCommand calibrateCommand)
        {
        }

        private async Task ControlDimmer(double destinationLevel)
        {
            int powerOnTime = 0;
            var dest = TimeForPercentage(destinationLevel);
            var current = TimeForPercentage(_PowerLevel.Value);

            if (destinationLevel > _PowerLevel)
            {
                // If last time dimmer was increasing its value we have to change direction by short time power on
                if (_CurrentValue > _PreviousCurrentValue && _CurrentValue > 0)
                {
                    // We add time that we consume on direction change
                    powerOnTime += await ChangeDimmerDirection();
                }

                powerOnTime += (int)(dest - current);

                Logger.LogInformation($"Set dimmer to {destinationLevel} by waiting {powerOnTime}ms");
            }
            else
            {
                // If last time dimmer was decreasing its value we have to change direction by short time power on
                if (_PreviousCurrentValue > _CurrentValue && _CurrentValue > 0)
                {
                    powerOnTime += await ChangeDimmerDirection();
                }

                powerOnTime += (int)(current - dest);
            }

            ForwardToPowerAdapter(TurnOnCommand.Create(powerOnTime));
        }

        private async Task<int> ChangeDimmerDirection()
        {
            ForwardToPowerAdapter(TurnOnCommand.Create(SWITCH_CHANGE_DIRECTION));

            await Task.Delay(WAIT_AFTER_CHANGE).ConfigureAwait(false);
            return SWITCH_CHANGE_DIRECTION;
        }

        /// <summary>
        /// Characteristic of dimmer read by measurements give equation of Time = 0.23 * percentage^2 + 500
        /// </summary>
        /// <param name="percentage"></param>
        /// <returns></returns>
        private double TimeForPercentage(double percentage)
        {
            if (percentage == 0) return 0;

            return (0.23 * percentage * percentage) + 500;
        }

        private double GetPowerLevel(double currentValue)
        {
            if (currentValue < _Minimum.Value) return 0;

            return ((currentValue - _Minimum.Value) / _Range.Value) * 100.0;
        }

        private async Task ChangePowerState()
        {
            ForwardToPowerAdapter(TurnOnCommand.Create(CHANGE_POWER_STATE_TIME));
            await Task.Delay(WAIT_AFTER_CHANGE);
        }

        private void ForwardToPowerAdapter(Command command)
        {
            command.SetProperty(MessageProperties.PinNumber, _PowerAdapterPin);

            MessageBroker.Send(command, _PowerAdapterUid);
        }

        private bool TryCalculateSpectrum()
        {
            if (_Minimum.HasValue && _Maximum.HasValue)
            {
                _Range = _Maximum.Value - _Minimum.Value;
                return true;
            }
            return false;
        }
    }
}