using System;
using System.Collections.Generic;
using System.Text;

namespace HomeCenter.Model.Exceptions
{

    public class MissingAdapterException : Exception
    {
        public MissingAdapterException() : base()
        {
        }

        protected MissingAdapterException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context)
        {
        }

        public MissingAdapterException(string message) : base(message)
        {
        }

        public MissingAdapterException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
