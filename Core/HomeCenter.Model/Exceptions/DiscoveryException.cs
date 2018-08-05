using System;

namespace HomeCenter.Contracts.Exceptions
{
    public class DiscoveryException : Exception
    {
        public DiscoveryException() : base()
        {
        }

        protected DiscoveryException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context)
        {
        }

        public DiscoveryException(string message) : base(message)
        {
        }

        public DiscoveryException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}