using System;

namespace HomeCenter.Model.Exceptions
{
    public class MissingTypeException : Exception
    {
        public MissingTypeException() : base()
        {
        }

        protected MissingTypeException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context)
        {
        }

        public MissingTypeException(string message) : base(message)
        {
        }

        public MissingTypeException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}