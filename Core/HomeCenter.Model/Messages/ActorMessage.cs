﻿using HomeCenter.Model.Core;

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
    }
}