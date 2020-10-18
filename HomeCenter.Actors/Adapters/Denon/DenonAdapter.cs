using HomeCenter.Adapters.Denon.Messages;
using HomeCenter.CodeGeneration;
using HomeCenter.Model.Adapters;
using HomeCenter.Model.Capabilities;
using HomeCenter.Model.Messages;
using HomeCenter.Model.Messages.Commands.Device;
using HomeCenter.Model.Messages.Events.Service;
using HomeCenter.Model.Messages.Queries.Device;
using HomeCenter.Model.Messages.Scheduler;
using HomeCenter.Extensions;
using Microsoft.Extensions.Logging;
using Proto;
using System;
using System.Threading.Tasks;

namespace HomeCenter.Adapters.Denon
{
    [ProxyCodeGenerator]
    public abstract class DenonAdapter : Adapter
    {
        public const int DEFAULT_POOL_INTERVAL = 2000;
        public const int DEFAULT_VOLUME_CHANGE_FACTOR = 10;

        private bool _powerState;
        private double? _volume;
        private bool _mute;
        private string _input;
        private string _surround;
        private DenonDeviceInfo _fullState;
        private string _description;

        private string _hostName;
        private int _zone;
        private TimeSpan _poolInterval;

        protected override async Task OnStarted(IContext context)
        {
            await base.OnStarted(context);

            _hostName = this.AsString(MessageProperties.Hostname);
            _poolInterval = this.AsIntTime(MessageProperties.PoolInterval, DEFAULT_POOL_INTERVAL);
            _zone = this.AsInt(MessageProperties.Zone);

            await MessageBroker.SendAfterDelay(ActorMessageContext.Create(Self, RefreshCommand.Default), TimeSpan.FromSeconds(1));
            await ScheduleDeviceLightRefresh(_poolInterval);
        }

        protected DiscoveryResponse Discover(DiscoverQuery message)
        {
            return new DiscoveryResponse(RequierdProperties(), new PowerState(),
                                                               new VolumeState(),
                                                               new MuteState(),
                                                               new InputSourceState(),
                                                               new SurroundSoundState(),
                                                               new DescriptionState()
                                          );
        }

        protected async Task Handle(RefreshCommand message)
        {
            _fullState = await MessageBroker.QueryService<DenonStatusQuery, DenonDeviceInfo>(new DenonStatusQuery { Address = _hostName });
            var mapping = await MessageBroker.QueryService<DenonMappingQuery, DenonDeviceInfo>(new DenonMappingQuery { Address = _hostName });
            _fullState.FriendlyName = mapping.FriendlyName;
            _fullState.InputMap = mapping.InputMap;

            _surround = await UpdateState(SurroundSoundState.StateName, _surround, _fullState.Surround);
            _description = await UpdateState(DescriptionState.StateName, _description, GetDescription());
        }

        private string GetDescription() => $"{_fullState.FriendlyName} [Model: {_fullState.Model}, Zone: {_zone}, Address: {_hostName}]";

        protected async Task Handle(RefreshLightCommand message)
        {
            var statusQuery = new DenonStatusLightQuery
            {
                Address = _hostName,
                Zone = _zone.ToString()
            };

            var state = await MessageBroker.QueryService<DenonStatusLightQuery, DenonStatus>(statusQuery);

            _input = await UpdateState(InputSourceState.StateName, _input, state.ActiveInput);
            _mute = await UpdateState(MuteState.StateName, _mute, state.Mute);
            _powerState = await UpdateState(PowerState.StateName, _powerState, state.PowerStatus);
            _volume = await UpdateState(VolumeState.StateName, _volume, state.MasterVolume);
        }

        protected async Task Handle(TurnOnCommand message)
        {
            var control = new DenonControlQuery
            {
                Command = "PowerOn",
                Api = "formiPhoneAppPower",
                ReturnNode = "Power",
                Address = _hostName,
                Zone = _zone.ToString()
            };

            if (await MessageBroker.QueryServiceWithVerify<DenonControlQuery, string, object>(control, "ON"))
            {
                _powerState = await UpdateState(PowerState.StateName, _powerState, true);
            }
        }

        protected async Task Handle(TurnOffCommand message)
        {
            var control = new DenonControlQuery
            {
                Command = "PowerStandby",
                Api = "formiPhoneAppPower",
                ReturnNode = "Power",
                Address = _hostName,
                Zone = _zone.ToString()
            };

            if (await MessageBroker.QueryServiceWithVerify<DenonControlQuery, string, object>(control, "OFF"))
            {
                _powerState = await UpdateState(PowerState.StateName, _powerState, false);
            }
        }

        protected async Task Handle(VolumeUpCommand command)
        {
            if (_volume.HasValue)
            {
                var changeFactor = command.AsDouble(MessageProperties.ChangeFactor, DEFAULT_VOLUME_CHANGE_FACTOR);
                var volume = _volume + changeFactor;

                var normalized = NormalizeVolume(volume.Value);

                var control = new DenonControlQuery
                {
                    Command = normalized,
                    Api = "formiPhoneAppVolume",
                    ReturnNode = "MasterVolume",
                    Address = _hostName,
                    Zone = _zone.ToString()
                };

                // Results are unpredictable so we ignore them
                await MessageBroker.QueryService<DenonControlQuery, string>(control);
                _volume = await UpdateState(VolumeState.StateName, _volume, volume);
            }
        }

        protected async Task Handle(VolumeDownCommand command)
        {
            if (_volume.HasValue)
            {
                var changeFactor = command.AsDouble(MessageProperties.ChangeFactor, DEFAULT_VOLUME_CHANGE_FACTOR);
                var volume = _volume - changeFactor;
                var normalized = NormalizeVolume(volume.Value);

                var control = new DenonControlQuery
                {
                    Command = normalized,
                    Api = "formiPhoneAppVolume",
                    ReturnNode = "MasterVolume",
                    Address = _hostName,
                    Zone = _zone.ToString()
                };

                // Results are unpredictable so we ignore them
                await MessageBroker.QueryService<DenonControlQuery, string>(control);
                _volume = await UpdateState(VolumeState.StateName, _volume, volume);
            }
        }

        protected async Task Handle(VolumeSetCommand command)
        {
            var volume = command.AsDouble(MessageProperties.Value);
            var normalized = NormalizeVolume(volume);

            var control = new DenonControlQuery
            {
                Command = normalized,
                Api = "formiPhoneAppVolume",
                ReturnNode = "MasterVolume",
                Address = _hostName,
                Zone = _zone.ToString()
            };

            await MessageBroker.QueryService<DenonControlQuery, string>(control);
            _volume = await UpdateState(VolumeState.StateName, _volume, volume);
        }

        private string NormalizeVolume(double volume)
        {
            if (volume < 0) volume = 0;
            if (volume > 100) volume = 100;

            return (volume - 80).ToFloatString();
        }

        protected async Task Handle(MuteCommand message)
        {
            var control = new DenonControlQuery
            {
                Command = "MuteOn",
                Api = "formiPhoneAppMute",
                ReturnNode = "Mute",
                Address = _hostName,
                Zone = _zone.ToString()
            };

            if (await MessageBroker.QueryServiceWithVerify<DenonControlQuery, string, object>(control, "on"))
            {
                _mute = await UpdateState(MuteState.StateName, _mute, true);
            }
        }

        protected async Task Handle(UnmuteCommand message)
        {
            var control = new DenonControlQuery
            {
                Command = "MuteOff",
                Api = "formiPhoneAppMute",
                ReturnNode = "Mute",
                Address = _hostName,
                Zone = _zone.ToString()
            };

            if (await MessageBroker.QueryServiceWithVerify<DenonControlQuery, string, object>(control, "off"))
            {
                _mute = await UpdateState(MuteState.StateName, _mute, false);
            }
        }

        protected async Task SetInput(InputSetCommand message)
        {
            if (_fullState == null) throw new ArgumentException("Cannot change input source on Denon device because device info was not downloaded from device");
            var inputName = message.AsString(MessageProperties.InputSource);
            var input = _fullState.TranslateInputName(inputName, _zone.ToString());
            if (input?.Length == 0) throw new ArgumentException($"Input {inputName} was not found on available device input sources");

            var control = new DenonControlQuery
            {
                Command = input,
                Api = "formiPhoneAppDirect",
                ReturnNode = "",
                Zone = "",
                Address = _hostName
            };

            await MessageBroker.QueryService<DenonControlQuery, string>(control);

            _input = await UpdateState(InputSourceState.StateName, _input, inputName);
        }

        protected async Task Handle(ModeSetCommand message)
        {
            //Surround support only in main zone
            if (_zone != 1) return;
            var surroundMode = message.AsString(MessageProperties.SurroundMode);
            var mode = DenonSurroundModes.MapApiCommand(surroundMode);
            if (mode?.Length == 0) throw new ArgumentException($"Surroundmode {mode} was not found on available surround modes");

            var control = new DenonControlQuery
            {
                Command = mode,
                Api = "formiPhoneAppDirect",
                ReturnNode = "",
                Zone = "",
                Address = _hostName
            };

            await MessageBroker.QueryService<DenonControlQuery, string>(control);

            _surround = await UpdateState(SurroundSoundState.StateName, _surround, surroundMode);
        }

    }
}