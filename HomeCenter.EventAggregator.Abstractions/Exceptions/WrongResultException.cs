using System;

namespace HomeCenter.EventAggregator.Exceptions
{
    public class WrongResultException : Exception
    {
        public WrongResultException(object? actual, object? excepted) : base($"Result value is {actual} but excepted value is {excepted}")
        {
        }

        public WrongResultException() : base()
        {
        }

        protected WrongResultException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context)
        {
        }

        public WrongResultException(string message) : base(message)
        {
        }

        public WrongResultException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}