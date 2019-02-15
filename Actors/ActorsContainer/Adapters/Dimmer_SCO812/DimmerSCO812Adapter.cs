using HomeCenter.Broker;
using HomeCenter.CodeGeneration;
using HomeCenter.Model.Adapters;
using HomeCenter.Model.Capabilities;
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
        private double _Minimum;
        private double _Maximum;
        private double _Spectrum;

        private double? _PowerLevel;

        protected DimmerSCO812Adapter()
        {
        }

        protected override async Task OnStarted(IContext context)
        {
            await base.OnStarted(context).ConfigureAwait(false);

            _PowerAdapterUid = AsString("PowerAdapter");
            _PowerAdapterPin = AsInt("PowerAdapterPin");
            _PowerLevelAdapterUid = AsString("PowerLevelAdapterUid");
            _PowerLevelAdapterPin = AsInt("PowerLevelAdapterPin");
            _TimeToFullLight = AsTime("TimeToFullLight");
            _Minimum = AsDouble("MinimumCurrent");
            _Maximum = AsDouble("MaximumCurrent");
            _Spectrum = _Maximum - _Minimum;

            await MessageBroker.Request<DiscoverQuery, DiscoveryResponse>((DiscoverQuery)DiscoverQuery.Default.SetProperty(MessageProperties.PinNumber, _PowerLevelAdapterPin), _PowerLevelAdapterUid);

            _disposables.Add(MessageBroker.SubscribeForEvent<PropertyChangedEvent>(CurrentChangeHanler, _PowerLevelAdapterUid));
        }

        private async Task CurrentChangeHanler(IMessageEnvelope<PropertyChangedEvent> message)
        {
            var newLevel = GetPowerLevel(message.Message.AsDouble(MessageProperties.NewValue));
            _PowerLevel = await UpdateState(PowerLevelState.StateName, _PowerLevel, newLevel);
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

        private double GetPowerLevel(double actual)
        {
            if (actual < _Minimum) return 0;

            return ((actual - _Minimum) / _Spectrum) * 100.0;
        }

        private void ForwardToPowerAdapter(Command command)
        {
            command.SetProperty(MessageProperties.PinNumber, _PowerAdapterPin)
                   .SetProperty(MessageProperties.StateTime, CHANGE_POWER_STATE_TIME);

            MessageBroker.Send(command, _PowerAdapterUid);
        }
    }
}