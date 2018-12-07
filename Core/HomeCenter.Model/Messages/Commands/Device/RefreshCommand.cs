namespace HomeCenter.Model.Messages.Commands.Device
{
    public class RefreshCommand : Command
    {
        public static RefreshCommand Default = new RefreshCommand();

        public RefreshCommand()
        {
            DefaultLogLevel = Microsoft.Extensions.Logging.LogLevel.Trace;
        }
    }
}