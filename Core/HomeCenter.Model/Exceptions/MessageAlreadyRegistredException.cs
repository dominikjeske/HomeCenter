using System;

namespace HomeCenter.Model.Exceptions
{
    public class MessageAlreadyRegistredException : Exception
    {
        public MessageAlreadyRegistredException() : base()
        {
        }

        protected MessageAlreadyRegistredException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context)
        {
        }

        public MessageAlreadyRegistredException(string message) : base(message)
        {
        }

        public MessageAlreadyRegistredException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}