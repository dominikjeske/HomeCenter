using System;

namespace HomeCenter.Model.Exceptions
{

    public class WrongPropertyFormatException : Exception
    {
        public WrongPropertyFormatException() : base()
        {
        }

        protected WrongPropertyFormatException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context)
        {
        }

        public WrongPropertyFormatException(string message) : base(message)
        {
        }

        public WrongPropertyFormatException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}