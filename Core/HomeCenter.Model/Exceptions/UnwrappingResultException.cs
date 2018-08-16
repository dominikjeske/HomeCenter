using System;

namespace HomeCenter.Model.Exceptions
{
    public class UnwrappingResultException : Exception
    {
        public UnwrappingResultException() : base()
        {
        }

        protected UnwrappingResultException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context)
        {
        }

        public UnwrappingResultException(string message) : base(message)
        {
        }

        public UnwrappingResultException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}