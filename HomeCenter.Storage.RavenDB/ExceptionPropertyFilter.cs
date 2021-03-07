using Serilog.Exceptions.Filters;
using System;

namespace HomeCenter.Storage.RavenDB
{
    public class ExceptionPropertyFilter : IExceptionPropertyFilter
    {
        public bool ShouldPropertyBeFiltered(Exception exception, string propertyName, object value)
        {
            if (string.Equals(propertyName, "HResult", StringComparison.OrdinalIgnoreCase)) return true;

            return false;
        }
    }
}