using HomeCenter.CodeGeneration;
using HomeCenter.Model.Adapters;
using HomeCenter.Model.Capabilities;
using HomeCenter.Model.Core;
using HomeCenter.Model.Messages;
using HomeCenter.Model.Messages.Commands;
using HomeCenter.Model.Messages.Commands.Device;
using HomeCenter.Model.Messages.Events.Device;
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

        private string _PowerAdapterUid;
        private int _PowerAdapterPin;
        private string _PowerLevelAdapterUid;
        private int _PowerLevelAdapterPin;
        private TimeSpan _TimeToFullLight;
        private double? _Minimum;
        private double? _Maximum;

        private double? _Range;
        private double? _PowerLevel;
        private double? _CurrentValue;
        private DateTimeOffset _Start;
        private DateTimeOffset _End;

        protected override async Task OnStarted(IContext context)
        {
            await base.OnStarted(context).ConfigureAwait(false);

            _PowerAdapterUid = AsString("PowerAdapter");
            _PowerAdapterPin = AsInt("PowerAdapterPin");
            _PowerLevelAdapterUid = AsString("PowerLevelAdapterUid");
            _PowerLevelAdapterPin = AsInt("PowerLevelAdapterPin");
            _TimeToFullLight = AsIntTime("TimeToFullLight");
            _Minimum = AsNullableDouble("Minimum");
            _Maximum = AsNullableDouble("Maximum");

            await MessageBroker.Request<DiscoverQuery, DiscoveryResponse>((DiscoverQuery)DiscoverQuery.Default.SetProperty(MessageProperties.PinNumber, _PowerLevelAdapterPin), _PowerLevelAdapterUid);

            _disposables.Add(MessageBroker.SubscribeForMessage<PropertyChangedEvent>(Self, _PowerLevelAdapterUid));

            if (!TryCalculateSpectrum())
            {
                Logger.LogWarning($"Calibration of {Uid} : Start");
                Become(CalibrationCheckState);

                // We change state to check in which state we are
                ForwardToPowerAdapter(TurnOnCommand.Create(CHANGE_POWER_STATE_TIME));
            }
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

        private async Task CalibrationCheckState(IContext context)
        {
            if (context.Message is PropertyChangedEvent property)
            {
                var newValue = property.AsDouble(MessageProperties.NewValue);
                var oldValue = property.AsDouble(MessageProperties.OldValue);

                // I we changed OFF => ON we have to turn off before start
                if (newValue > oldValue)
                {
                    Logger.LogWarning($"Calibration of {Uid} : detect ON state. Dimmer will be turned off");

                    ForwardToPowerAdapter(TurnOnCommand.Create(CHANGE_POWER_STATE_TIME));

                    await Task.Delay(CHANGE_POWER_STATE_TIME * 4);
                }

                Logger.LogWarning($"Calibration of {Uid} : waiting to reach MAX state");

                Become(CalibrationMaximumLight);

                _Start = SystemTime.Now;
                ForwardToPowerAdapter(TurnOnCommand.Default);

                return;
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
                Logger.LogWarning($"Calibration of {Uid} : MAX was read after {SystemTime.Now - _Start}");

                await Task.Delay(500);

                _Start = SystemTime.Now;
                Logger.LogWarning($"Calibration of {Uid} : waiting to reach MIN state");

                ForwardToPowerAdapter(TurnOnCommand.Default);

                return;
            }
            else if (context.Message is PropertyChangedEvent maximumState)
            {
                // Resend stop message to cancel scheduled message
                await MessageBroker.SendAfterDelay(ActorMessageContext.Create(Self, StopCommand.Create("MAX")), TimeSpan.FromMilliseconds(500), true).ConfigureAwait(false);

                _Maximum = maximumState.AsDouble(MessageProperties.NewValue);

                Logger.LogWarning($"Calibration of {Uid} : new MAX was received {_Maximum}");
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
                if (stopCommand[MessageProperties.Context] == "MAX")
                {
                    return;
                }

                Become(StandardMode);

                ForwardToPowerAdapter(TurnOffCommand.Default);
                _TimeToFullLight = SystemTime.Now - _Start;
                Logger.LogWarning($"Calibration of {Uid} : MIN was read after {_TimeToFullLight}");

                TryCalculateSpectrum();

                Logger.LogWarning($"Calibration of {Uid} : calibration finished with MIN: {_Minimum}, MAX: {_Maximum}, RANGE: {_Range}, TIME: {_TimeToFullLight}");

                await Task.Delay(500);

                ForwardToPowerAdapter(TurnOnCommand.Create(CHANGE_POWER_STATE_TIME));
            }
            else if (context.Message is PropertyChangedEvent minimumState)
            {
                await MessageBroker.SendAfterDelay(ActorMessageContext.Create(Self, StopCommand.Create("MIN")), TimeSpan.FromMilliseconds(1000)).ConfigureAwait(false);

                _Minimum = minimumState.AsDouble(MessageProperties.NewValue);

                Logger.LogWarning($"Calibration of {Uid} : new MIN was received {_Minimum}");
            }
            else
            {
                await StandardMode(context).ConfigureAwait(false);
            }
        }

        protected async Task Handle(PropertyChangedEvent propertyChangedEvent)
        {
            _CurrentValue = propertyChangedEvent.AsDouble(MessageProperties.NewValue);
            var newLevel = GetPowerLevel(_CurrentValue.Value);

            if (newLevel.HasValue)
            {
                _PowerLevel = await UpdateState(PowerLevelState.StateName, _PowerLevel, newLevel);
            }
        }

        protected DiscoveryResponse Discover(DiscoverQuery message)
        {
            return new DiscoveryResponse(RequierdProperties(), new PowerLevelState(), new PowerState());
        }

        protected void Handle(TurnOnCommand turnOnCommand)
        {
            ForwardToPowerAdapter((Command)turnOnCommand.SetProperty(MessageProperties.StateTime, CHANGE_POWER_STATE_TIME));
        }

        protected void Handle(TurnOffCommand turnOnCommand)
        {
            ForwardToPowerAdapter(turnOnCommand);
        }

        protected void Handle(SetPowerLevelCommand powerLevel)
        {
            var destinationLevel = powerLevel.PowerLevel;

            ControlDimmer(destinationLevel);
        }

        protected void Handle(CalibrateCommand calibrateCommand)
        {
        }

        private void ControlDimmer(double destinationLevel)
        {
            int powerOnTime = 0;

            if (destinationLevel > _PowerLevel)
            {
                var diff = destinationLevel - _PowerLevel;

                //TODO linear for start
                powerOnTime = (int)(diff / 100.0 * _TimeToFullLight.TotalMilliseconds);
            }
            else
            {
                MessageBroker.Send(TurnOffCommand.Default, _PowerAdapterUid);

                powerOnTime = (int)(destinationLevel / 100.0 * _TimeToFullLight.TotalMilliseconds);
            }

            MessageBroker.Send(TurnOnCommand.Create(powerOnTime), _PowerAdapterUid);
        }

        protected void Handle(AdjustPowerLevelCommand powerLevel)
        {
            if (!_PowerLevel.HasValue) return;

            var destinationLevel = _PowerLevel.Value + powerLevel.Delta;

            ControlDimmer(destinationLevel);
        }

        private double? GetPowerLevel(double actual)
        {
            if (actual < _Minimum) return 0;
            if (!_Minimum.HasValue || !_Range.HasValue) return null;

            return ((actual - _Minimum.Value) / _Range.Value) * 100.0;
        }

        private void ForwardToPowerAdapter(Command command)
        {
            command.SetProperty(MessageProperties.PinNumber, _PowerAdapterPin);

            MessageBroker.Send(command, _PowerAdapterUid);
        }
    }
}