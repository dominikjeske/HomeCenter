using System;

namespace HomeCenter.Model.Exceptions
{
    public class UnsupportedMessageException : Exception
    {
        public UnsupportedMessageException() : base()
        {
        }

        protected UnsupportedMessageException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context)
        {
        }

        public UnsupportedMessageException(string message) : base(message)
        {
        }

        public UnsupportedMessageException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}