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

    public class FakeII2cBus : II2cBus
    {
        public void Read(int address, byte[] buffer)
        {
        }

        public void Write(int address, byte[] data)
        {
        }

        public void WriteRead(int deviceAddress, byte[] writeBuffer, byte[] readBuffer)
        {
        }
    }
}