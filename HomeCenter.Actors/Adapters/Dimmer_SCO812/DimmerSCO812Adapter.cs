using System;
using System.Threading.Tasks;
using HomeCenter.Abstractions;
using HomeCenter.Actors.Core;
using HomeCenter.Capabilities;
using HomeCenter.Messages.Commands.Device;
using HomeCenter.Messages.Events.Device;
using HomeCenter.Messages.Events.Service;
using HomeCenter.Messages.Queries.Device;
using Microsoft.Extensions.Logging;
using Proto;

namespace HomeCenter.Adapters.CurrentBridge
{
    [Proxy]
    public class DimmerSCO812Adapter : Adapter
    {
        private const int CHANGE_POWER_STATE_TIME = 200;
        private const int SWITCH_CHANGE_DIRECTION = 400;
        private const int WAIT_AFTER_CHANGE = 500;

        private string? _powerAdapterUid;
        private int _powerAdapterPin;
        private string? _powerLevelAdapterUid;
        private int _powerLevelAdapterPin;
        private double? _minimum;
        private double? _maximum;

        private double? _range;
        private double? _powerLevel;
        private double? _currentValue;
        private double? _previousCurrentValue;

        protected override async Task OnStarted(IContext context)
        {
            await base.OnStarted(context);

            _powerAdapterUid = this.AsString("PowerAdapter");
            _powerAdapterPin = this.AsInt("PowerAdapterPin");
            _powerLevelAdapterUid = this.AsString("PowerLevelAdapterUid");
            _powerLevelAdapterPin = this.AsInt("PowerLevelAdapterPin");
            _minimum = this.AsNullableDouble("Minimum");
            _maximum = this.AsNullableDouble("Maximum");

            await MessageBroker.Request<DiscoverQuery, DiscoveryResponse>((DiscoverQuery)DiscoverQuery.Default.SetProperty(MessageProperties.PinNumber, _powerLevelAdapterPin), _powerLevelAdapterUid);

            ProtectResource(MessageBroker.SubscribeForMessage<PropertyChangedEvent>(Self, false, _powerLevelAdapterUid));
        }

        protected override async Task OnSystemStarted(SystemStartedEvent systemStartedEvent)
        {
            await base.OnSystemStarted(systemStartedEvent);

            if (!TryCalculateSpectrum())
            {
                Logger.LogInformation("[{Uid}] Calibration start", Uid);
                Become(CalibrationFirstStateCheck);
                await ChangePowerState();
            }
        }

        private async Task CalibrationFirstStateCheck(IContext context)
        {
            if (context.Message is PropertyChangedEvent property)
            {
                _currentValue = property.AsDouble(MessageProperties.NewValue);
                Become(CalibrationSecondStateCheck);
                await ChangePowerState();
            }
            else
            {
                await StandardMode(context);
            }
        }

        private async Task CalibrationSecondStateCheck(IContext context)
        {
            if (context.Message is PropertyChangedEvent property)
            {
                var newValue = property.AsDouble(MessageProperties.NewValue);

                // I we changed OFF => ON we have to turn off before start
                if (newValue > _currentValue)
                {
                    Logger.LogInformation("[{Uid}] Calibration detected ON state. Dimmer will be turned off", Uid);
                    await ChangePowerState();
                }

                Logger.LogInformation("[{Uid}] Calibration waiting to reach MAX state", Uid);
                Become(CalibrationMaximumLight);
                ForwardToPowerAdapter(TurnOnCommand.Default);
            }
            else
            {
                await StandardMode(context);
            }
        }

        private async Task CalibrationMaximumLight(IContext context)
        {
            if (context.Message is StopCommand stopCommand)
            {
                Become(CalibrationMinimumLight);
                ForwardToPowerAdapter(TurnOffCommand.Default);
                await Task.Delay(WAIT_AFTER_CHANGE);

                Logger.LogInformation("[{Uid}] Calibration waiting to reach MIN state", Uid);
                ForwardToPowerAdapter(TurnOnCommand.Default);
            }
            else if (context.Message is PropertyChangedEvent maximumState)
            {
                // Resend stop message to cancel scheduled message
                await MessageBroker.SendAfterDelay(ActorMessageContext.Create(Self, StopCommand.Create("MAX")), TimeSpan.FromMilliseconds(1500), true);
                _maximum = maximumState.AsDouble(MessageProperties.NewValue);
            }
            else
            {
                await StandardMode(context);
            }
        }

        private async Task CalibrationMinimumLight(IContext context)
        {
            if (context.Message is StopCommand stopCommand)
            {
                // Ignore previous commands if we receive any
                if (stopCommand[MessageProperties.Context].ToString() == "MAX")
                {
                    return;
                }

                Become(StandardMode);

                ForwardToPowerAdapter(TurnOffCommand.Default);

                ResetStateValues();

                TryCalculateSpectrum();

                Logger.LogInformation("[{Uid}] Calibration finished with MIN: {Minimum}, MAX: {Maximum}, RANGE: {Range}", Uid, _minimum, _maximum, _range);

                await Task.Delay(WAIT_AFTER_CHANGE);

                ForwardToPowerAdapter(TurnOnCommand.Create(CHANGE_POWER_STATE_TIME));
            }
            else if (context.Message is PropertyChangedEvent minimumState)
            {
                await MessageBroker.SendAfterDelay(ActorMessageContext.Create(Self, StopCommand.Create("MIN")), TimeSpan.FromMilliseconds(1500));
                _minimum = minimumState.AsDouble(MessageProperties.NewValue);
            }
            else
            {
                await StandardMode(context);
            }
        }

        private void ResetStateValues()
        {
            _currentValue = 0;
            _previousCurrentValue = 0;
            _powerLevel = 0;
        }

        protected async Task Handle(PropertyChangedEvent propertyChangedEvent)
        {
            if (!_minimum.HasValue || !_range.HasValue)
            {
                return;
            }

            var value = propertyChangedEvent.AsDouble(MessageProperties.NewValue);
            if (value < _minimum)
            {
                value = 0;
            }

            _previousCurrentValue = _currentValue;
            _currentValue = value;

            var newLevel = GetPowerLevel(value);

            Logger.LogInformation("NEW: {newLevel}, MAX: {Maximum}, MIN: {Minimum}", newLevel, _maximum, _minimum);

            _powerLevel = await UpdateState(PowerLevelState.StateName, _powerLevel, newLevel);
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
                if (_powerLevel > 0)
                {
                    return;
                }

                turnOnCommand.SetProperty(MessageProperties.StateTime, CHANGE_POWER_STATE_TIME);
            }

            ForwardToPowerAdapter(turnOnCommand);
        }

        protected async Task Handle(TurnOffCommand turnOnCommand)
        {
            if (_powerLevel == 0)
            {
                return;
            }

            await ChangePowerState();
        }

        protected Task Handle(SetPowerLevelCommand powerLevel)
        {
            var destinationLevel = powerLevel.PowerLevel;

            if (!_powerLevel.HasValue)
            {
                return Task.CompletedTask;
            }

            return ControlDimmer(destinationLevel);
        }

        protected async Task Handle(AdjustPowerLevelCommand powerLevel)
        {
            if (!_powerLevel.HasValue)
            {
                return;
            }

            var destinationLevel = _powerLevel.Value + powerLevel.Delta;

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
            var current = TimeForPercentage(_powerLevel.GetValueOrDefault());

            if (destinationLevel > _powerLevel)
            {
                // If last time dimmer was increasing its value we have to change direction by short time power on
                if (_currentValue > _previousCurrentValue && _currentValue > 0)
                {
                    // We add time that we consume on direction change
                    powerOnTime += await ChangeDimmerDirection();
                }

                powerOnTime += (int)(dest - current);

                Logger.LogInformation($"[{Uid}] Set dimmer to {destinationLevel} by waiting {powerOnTime}ms");
            }
            else
            {
                // If last time dimmer was decreasing its value we have to change direction by short time power on
                if (_previousCurrentValue > _currentValue && _currentValue > 0)
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

            await Task.Delay(WAIT_AFTER_CHANGE);
            return SWITCH_CHANGE_DIRECTION;
        }

        /// <summary>
        /// Characteristic of dimmer read by measurements give equation of Time = 0.23 * percentage^2 + 500.
        /// </summary>
        /// <param name="percentage"></param>
        /// <returns></returns>
        private double TimeForPercentage(double percentage)
        {
            if (percentage == 0)
            {
                return 0;
            }

            return (0.23 * percentage * percentage) + 500;
        }

        private double GetPowerLevel(double currentValue)
        {
            if (currentValue < _minimum.GetValueOrDefault())
            {
                return 0;
            }

            return ((currentValue - _minimum.GetValueOrDefault()) / _range.GetValueOrDefault()) * 100.0;
        }

        private async Task ChangePowerState()
        {
            ForwardToPowerAdapter(TurnOnCommand.Create(CHANGE_POWER_STATE_TIME));
            await Task.Delay(WAIT_AFTER_CHANGE);
        }

        private void ForwardToPowerAdapter(Command command)
        {
            if (_powerAdapterUid is null)
            {
                throw new InvalidOperationException();
            }

            command.SetProperty(MessageProperties.PinNumber, _powerAdapterPin);

            MessageBroker.Send(command, _powerAdapterUid);
        }

        private bool TryCalculateSpectrum()
        {
            if (_minimum.HasValue && _maximum.HasValue)
            {
                _range = _maximum.Value - _minimum.Value;
                return true;
            }

            return false;
        }
    }
}