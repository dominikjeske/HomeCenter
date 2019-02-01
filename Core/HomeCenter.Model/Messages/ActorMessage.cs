using HomeCenter.Model.Core;
using HomeCenter.Utils.Extensions;
using System;

namespace HomeCenter.Model.Messages
{
    public class ActorMessage : BaseObject, IEquatable<ActorMessage>
    {
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