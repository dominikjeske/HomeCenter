using HomeCenter.Model.Core;

namespace HomeCenter.Model.Messages
{
    public class ActorMessage : BaseObject
    {
        public Proto.IContext Context { get; set; }

        public string LogLevel
        {
            get => AsString(MessageProperties.LogLevel, nameof(Microsoft.Extensions.Logging.LogLevel.Information));
            set => SetProperty(MessageProperties.LogLevel, value);
        }
    }
}