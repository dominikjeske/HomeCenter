using HomeCenter.Model.Core;
using HomeCenter.Utils.Extensions;
using System;
using System.Text.Json.Serialization;

namespace HomeCenter.Model.Messages
{
    public class ActorMessage : BaseObject, IEquatable<ActorMessage>
    {
        [JsonIgnore]
        public Proto.IContext Context { get; set; }

        public string LogLevel
        {
            get => AsString(MessageProperties.LogLevel, nameof(Microsoft.Extensions.Logging.LogLevel.Information));
            set => SetProperty(MessageProperties.LogLevel, value);
        }

        public bool Equals(ActorMessage other)
        {
            return other != null && GetProperties().LeftEqual(other.GetProperties());
        }
    }
}