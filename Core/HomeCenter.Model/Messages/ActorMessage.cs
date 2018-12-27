using HomeCenter.Broker;
using HomeCenter.Model.Core;
using HomeCenter.Utils.Extensions;
using System.Linq;

namespace HomeCenter.Model.Messages
{
    public abstract class ActorMessage : BaseObject
    {
        public Proto.IContext Context { get; set; }

        protected ActorMessage()
        {
            Type = GetType().Name;
        }

        public string LogLevel
        {
            get => AsString(MessageProperties.LogLevel, nameof(Microsoft.Extensions.Logging.LogLevel.Information));
            set => SetProperty(MessageProperties.LogLevel, value);
        }

        public RoutingFilter GetRoutingFilterFromProperties()
        {
            var attributes = GetProperties().ToDictionary();
            attributes.Add(MessageProperties.Type, Type);

            return new RoutingFilter(attributes);
        }
    }
}