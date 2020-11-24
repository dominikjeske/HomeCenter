using HomeCenter.Abstractions;
using HomeCenter.Abstractions.Defaults;
using HomeCenter.Messages.Commands.Device;
using System.Diagnostics.CodeAnalysis;

namespace HomeCenter.Capabilities
{
    public class PlaybackState : StateBase
    {
        public static string StateName { get; } = nameof(PlaybackState);

        public PlaybackState([AllowNull] string ReadWriteMode = default) : base(ReadWriteMode)
        {
            this[StateProperties.StateName] = nameof(PlaybackState);
            this[StateProperties.CapabilityName] = Abstractions.Defaults.Capabilities.PlaybackController;
            this.SetPropertyList(StateProperties.SupportedCommands, nameof(PlayCommand), nameof(StopCommand));
        }
    }
}