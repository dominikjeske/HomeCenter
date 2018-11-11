using System;

namespace HomeCenter.Model.Exceptions
{

    public class MissingPropertyException : Exception
    {
        public MissingPropertyException() : base()
        {
        }

        protected MissingPropertyException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context)
        {
        }

        public MissingPropertyException(string message) : base(message)
        {
        }

        public MissingPropertyException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}