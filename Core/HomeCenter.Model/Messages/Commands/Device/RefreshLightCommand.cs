namespace HomeCenter.Model.Messages.Commands.Device
{
    public class RefreshLightCommand : Command
    {
        public static RefreshLightCommand Default = new RefreshLightCommand();

        public RefreshLightCommand()
        {
            DefaultLogLevel = Microsoft.Extensions.Logging.LogLevel.Trace;
        }
    }
}