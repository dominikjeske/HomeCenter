namespace Wirehome.ComponentModel.Commands
{
    public static class CommandType
    {
        public const string DiscoverCapabilities = nameof(DiscoverCapabilities);

        //Refresh full device state
        public const string RefreshCommand = nameof(RefreshCommand);

        // Refresh only part of the states
        public const string RefreshLightCommand = nameof(RefreshLightCommand);

        // Power
        public const string TurnOffCommand = nameof(TurnOffCommand);

        public const string TurnOnCommand = nameof(TurnOnCommand);
        public const string SwitchPowerCommand = nameof(SwitchPowerCommand);

        // Volume
        public const string VolumeUpCommand = nameof(VolumeUpCommand);
        public const string VolumeDownCommand = nameof(VolumeDownCommand);
        public const string VolumeSetCommand = nameof(VolumeSetCommand);

        //Mute
        public const string MuteCommand = nameof(MuteCommand);
        public const string UnmuteCommand = nameof(UnmuteCommand);

        // Input
        public const string SelectInputCommand = nameof(SelectInputCommand);

        // Surround
        public const string SelectSurroundModeCommand = nameof(SelectSurroundModeCommand);

        // Playback
        public const string PlayCommand = nameof(PlayCommand);
        public const string StopCommand = nameof(StopCommand);

        //Code
        public const string SendCode = nameof(SendCode);


        // Component
        public const string SupportedCapabilitiesCommand = nameof(SupportedCapabilitiesCommand);
        public const string SupportedStatesCommand = nameof(SupportedStatesCommand);
        public const string SupportedTagsCommand = nameof(SupportedTagsCommand);
        public const string GetStateCommand = nameof(GetStateCommand);

        //Controller
        public const string GetComponentCommand = nameof(GetComponentCommand);

        //Controller
        public const string GetSunriseCommand = nameof(GetSunriseCommand);
        public const string GetSunsetCommand = nameof(GetSunsetCommand);
    }
}