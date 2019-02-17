using HomeCenter.CodeGeneration;
using HomeCenter.Model.Adapters;
using HomeCenter.Model.Capabilities;
using HomeCenter.Model.Extensions;
using HomeCenter.Model.Messages;
using HomeCenter.Model.Messages.Commands;
using HomeCenter.Model.Messages.Commands.Device;
using HomeCenter.Model.Messages.Events.Device;
using HomeCenter.Model.Messages.Queries.Device;
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

        private Behavior _behavior = new Behavior();
        private double? _Spectrum;
        private double? _PowerLevel;
        private double? _CurrentValue;

        protected DimmerSCO812Adapter()
        {
            _behavior.Become(StandardMode);
        }

        public override Task ReceiveAsyncInternal(IContext context) => _behavior.ReceiveAsync(context);

        protected Task StandardMode(IContext context) => base.ReceiveAsyncInternal(context);

        protected override async Task OnStarted(IContext context)
        {
            await base.OnStarted(context).ConfigureAwait(false);

            _PowerAdapterUid = AsString("PowerAdapter");
            _PowerAdapterPin = AsInt("PowerAdapterPin");
            _PowerLevelAdapterUid = AsString("PowerLevelAdapterUid");
            _PowerLevelAdapterPin = AsInt("PowerLevelAdapterPin");
            _TimeToFullLight = AsTime("TimeToFullLight");
            _Minimum = AsNullableDouble("Minimum");
            _Maximum = AsNullableDouble("Maximum");

            await MessageBroker.Request<DiscoverQuery, DiscoveryResponse>((DiscoverQuery)DiscoverQuery.Default.SetProperty(MessageProperties.PinNumber, _PowerLevelAdapterPin), _PowerLevelAdapterUid);

            _disposables.Add(MessageBroker.SubscribeForMessage<PropertyChangedEvent>(Self, _PowerLevelAdapterUid));

            if (TryCalculateSpectrum())
            {
                _behavior.Become(CalibrationCheckState);

                // We change state to check in which state we are
                ForwardToPowerAdapter(TurnOnCommand.Create(CHANGE_POWER_STATE_TIME));
            }
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
                    ForwardToPowerAdapter(TurnOnCommand.Create(CHANGE_POWER_STATE_TIME));
                }

                var longStateCommand = TurnOnCommand.Create((int)_TimeToFullLight.TotalMilliseconds);

                _behavior.Become(CalibrationMaximumLight);

                ForwardToPowerAdapter(longStateCommand);

                await Scheduler.DelayCommandExecution(_TimeToFullLight + TimeSpan.FromMilliseconds(200), longStateCommand, Self.Id);

                return;
            }

            await UnhandledMessage(context.Message);
        }

        private async Task CalibrationMaximumLight(IContext context)
        {
            if (context.Message is TurnOnCommand longStateCommand)
            {
                _behavior.Become(CalibrationMinimumLight);

                ForwardToPowerAdapter(longStateCommand);

                await Scheduler.DelayCommandExecution(_TimeToFullLight + TimeSpan.FromMilliseconds(200), longStateCommand, Self.Id);

                return;
            }
            else if (context.Message is PropertyChangedEvent maximumState)
            {
                _Maximum = maximumState.AsDouble(MessageProperties.NewValue);
            }

            await UnhandledMessage(context.Message);
        }

        private Task CalibrationMinimumLight(IContext context)
        {
            if (context.Message is TurnOnCommand longStateCommand)
            {
                _behavior.Become(StandardMode);

                TryCalculateSpectrum();

                ForwardToPowerAdapter(TurnOnCommand.Create(CHANGE_POWER_STATE_TIME));

                return Task.CompletedTask;
            }
            else if (context.Message is PropertyChangedEvent minimumState)
            {
                _Minimum = minimumState.AsDouble(MessageProperties.NewValue);
            }

            return UnhandledMessage(context.Message);
        }

        private bool TryCalculateSpectrum()
        {
            if (_Minimum.HasValue && _Maximum.HasValue)
            {
                _Spectrum = _Maximum.Value - _Minimum.Value;
                return true;
            }
            return false;
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
            ForwardToPowerAdapter(turnOnCommand);
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
            if (!_Minimum.HasValue || !_Spectrum.HasValue) return null;

            return ((actual - _Minimum.Value) / _Spectrum.Value) * 100.0;
        }

        private void ForwardToPowerAdapter(Command command)
        {
            command.SetProperty(MessageProperties.PinNumber, _PowerAdapterPin);

            MessageBroker.Send(command, _PowerAdapterUid);
        }
    }
}