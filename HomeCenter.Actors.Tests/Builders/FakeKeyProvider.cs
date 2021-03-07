using HomeCenter.Storage.RavenDB;
using Microsoft.Extensions.Hosting;
using Raven.Client.Documents;
using Raven.Client.Documents.Conventions;
using Raven.Client.Json.Serialization.NewtonsoftJson;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using Serilog.Exceptions;
using Serilog.Exceptions.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Concurrency;
using System.Runtime.CompilerServices;
using System.Text;

namespace HomeCenter.Actors.Tests.Builders
{

    internal class FakeKeyProvider : LogKeyProvider
    {
        public override string GetKey() => Guid.NewGuid().ToString();
    }
}