using HomeCenter.Model.Core;
using HomeCenter.Utils.Extensions;
using Newtonsoft.Json;
using System;

namespace HomeCenter.Model.Messages
{
    public class ActorMessage : BaseObject, IEquatable<ActorMessage>
    {
        //TODO https://stackoverflow.com/questions/18543482/is-there-a-way-to-ignore-get-only-properties-in-json-net-without-using-jsonignor
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