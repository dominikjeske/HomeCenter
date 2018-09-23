using System;

namespace HomeCenter.Model.Exceptions
{
    public class UnsupportedResultException : Exception
    {
        public UnsupportedResultException() : base()
        {
        }

        protected UnsupportedResultException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context)
        {
        }

        public UnsupportedResultException(string message) : base(message)
        {
        }

        public UnsupportedResultException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
