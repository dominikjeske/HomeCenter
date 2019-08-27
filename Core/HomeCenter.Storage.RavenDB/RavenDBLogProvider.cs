using Microsoft.Extensions.Logging;
using System;

namespace HomeCenter.Storage.RavenDB
{
    public class RavenDBLogProvider : ILoggerProvider
    {
        public ILogger CreateLogger(string categoryName)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}