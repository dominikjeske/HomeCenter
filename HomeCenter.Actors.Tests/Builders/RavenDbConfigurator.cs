using Destructurama;
using HomeCenter.Storage.RavenDB;
using Microsoft.Extensions.Hosting;
using Raven.Client.Documents;
using Raven.Client.Documents.Conventions;
using Raven.Client.Json.Serialization.NewtonsoftJson;
using Raven.Client.ServerWide.Operations;
using Serilog;
using Serilog.Exceptions;
using Serilog.Exceptions.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Concurrency;

namespace HomeCenter.Actors.Tests.Builders
{
    internal class RavenDbConfigurator : IDisposable
    {
        private DocumentStore _docStore;
        private RavenTestLogEventSink? _ravenLogEventSink;

        public RavenDbConfigurator()
        {
            _docStore = new DocumentStore
            {
                Urls = new[]
                            {
                    "http://127.0.0.1:8080"
                },
                Database = "Logs",
                Conventions = new DocumentConventions
                {
                    Serialization = new NewtonsoftJsonSerializationConventions
                    {
                        CustomizeJsonSerializer = serializer =>
                        {
                            serializer.TypeNameHandling = Newtonsoft.Json.TypeNameHandling.None;
                        }
                    }
                }
            };

            _docStore.Initialize();
        }

        public bool CheckDbConnection()
        {
            try
            {
                var ServerHealthCheck = new GetBuildNumberOperation();
                _docStore.Maintenance.Server.Send(ServerHealthCheck);

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public void Configure(HostBuilderContext hostBuilderContext, LoggerConfiguration loggerConfiguration, IScheduler scheduler)
        {
            _ravenLogEventSink = new RavenTestLogEventSink(_docStore, new FakeKeyProvider(), new RavenSerializer());

            loggerConfiguration.Enrich.WithExceptionDetails(new DestructuringOptionsBuilder().WithFilter(new ExceptionPropertyFilter()))
                               .Enrich.FromLogContext()
                               .Enrich.With(new RxTimeEnricher(scheduler))
                               .Destructure.UsingAttributes()
                               .MinimumLevel.Verbose()
                               .WriteTo.Sink(_ravenLogEventSink);
        }

        public void Clear()
        {
            using var dbSession = _docStore.OpenSession();
            var objects = dbSession?.Query<Dictionary<string, object>>();
            while (objects?.Any() ?? false)
            {
                foreach (var obj in objects)
                {
                    dbSession?.Delete(obj);
                }

                dbSession?.SaveChanges();
            }
        }

        public void Dispose()
        {
            _ravenLogEventSink?.Dispose();
        }
    }
}