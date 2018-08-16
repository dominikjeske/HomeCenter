using System;

namespace HomeCenter.Model.Exceptions
{
    public class UnsupportedPropertyStateException : Exception
    {
        public UnsupportedPropertyStateException() : base()
        {
        }

        protected UnsupportedPropertyStateException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context)
        {
        }

        public UnsupportedPropertyStateException(string message) : base(message)
        {
        }

        public UnsupportedPropertyStateException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}