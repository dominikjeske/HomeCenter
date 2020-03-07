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

    public class FakeGpioDevice : IGpioDevice
    {
        public IObservable<PinChanged> PinChanged => Observable.Empty<PinChanged>();

        public void Dispose()
        {
        }

        public void RegisterPinChanged(int pinNumber, string pinMode)
        {
        }

        public void Write(int pin, bool value)
        {
        }
    }
}