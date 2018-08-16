using System;
using System.Collections.Generic;
using System.Text;

namespace HomeCenter.Model.Exceptions
{
    public class MissingConverterException : Exception
    {
        public MissingConverterException() : base()
        {
        }

        protected MissingConverterException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context)
        {
        }

        public MissingConverterException(string message) : base(message)
        {
        }

        public MissingConverterException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
