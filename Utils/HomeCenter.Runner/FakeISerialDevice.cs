using HomeCenter.Model.Contracts;
using HomeCenter.Services.Bootstrapper;
using HomeCenter.Services.Controllers;
using HomeCenter.Storage.RavenDB;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Raven.Client.Documents;
using Raven.Client.Exceptions.Database;
using Raven.Client.ServerWide;
using Raven.Client.ServerWide.Operations;
using Serilog;
using Serilog.Exceptions;
using Serilog.Exceptions.Core;
using Serilog.Exceptions.Filters;
using SimpleInjector;
using System;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;

namespace HomeCenter.Runner
{

    public class FakeISerialDevice : ISerialDevice
    {
        public void Dispose()
        {
        }

        public void Init()
        {
        }

        public void Send(byte[] data)
        {
        }

        public void Send(string data)
        {
        }

        public IDisposable Subscribe(Action<byte[]> handler) => Disposable.Empty;
    }
}