using System;

namespace HomeCenter.Model.Exceptions
{
    public class UnsupportedStateException : Exception
    {
        public UnsupportedStateException() : base()
        {
        }

        protected UnsupportedStateException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context)
        {
        }

        public UnsupportedStateException(string message) : base(message)
        {
        }

        public UnsupportedStateException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}