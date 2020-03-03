using HomeCenter.Model.Contracts;
using HomeCenter.Services.Bootstrapper;
using HomeCenter.Services.Controllers;
using HomeCenter.Storage.RavenDB;
using Microsoft.Extensions.Logging;
using Raven.Client.Documents;
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
    public class FakeBootstrapper : Bootstrapper
    {
        public FakeBootstrapper(Container container) : base(container)
        {
            _container.Options.AllowOverridingRegistrations = true;
        }

        protected override ILoggerProvider[] GetLogProviders()
        {
            var providers = base.GetLogProviders().ToList();
            return providers.ToArray();
        }

        public static IDocumentStore EnsureExists(IDocumentStore store)
        {
            try
            {
                using (var dbSession = store.OpenSession())
                {
                    dbSession.Query<LogEntry>().Take(0).ToList();
                }
            }
            catch (Raven.Client.Exceptions.Database.DatabaseDoesNotExistException)
            {
                store.Maintenance.Server.Send(new Raven.Client.ServerWide.Operations.CreateDatabaseOperation(new Raven.Client.ServerWide.DatabaseRecord
                {
                    DatabaseName = store.Database
                }));
            }

            return store;
        }

        private IDocumentStore CreateRavenDocStore()
        {
            var docStore = new DocumentStore
            {
                Urls = new[]
                {
                    "http://127.0.0.1:8080"
                },
                Database = "Logs",
                Conventions = { }
            };

            docStore.Conventions.CustomizeJsonSerializer = s => s.TypeNameHandling = Newtonsoft.Json.TypeNameHandling.None;
            docStore.Initialize();

            Log.Logger = new LoggerConfiguration()
                            .Enrich.WithExceptionDetails(new DestructuringOptionsBuilder().WithDefaultDestructurers()
                                                                                          .WithFilter(new IgnorePropertyByNameExceptionFilter(nameof(Exception.HResult))))
                            .WriteTo.Console()
                            .WriteTo.RavenDB(docStore)
                            .CreateLogger();

            //TESTS
            using (var dbSession = docStore.OpenSession())
            {
                //var test = new Dictionary<string, string>() { ["imie"] = "Dominik", ["nazwisko"] = "Jeske" };
                //dbSession.Store(test, "Move/");

                //var meta = dbSession.Advanced.GetMetadataFor(test);
                //meta["@collection"] = "Move";

                //var a = new LogMessage<TestClass>();
                //a.Value = testEntry;
                //dbSession.Store(a, "Move/");

                dbSession.SaveChanges();
            }

            return docStore;
        }

        protected override void RegisterNativeServices()
        {
            _container.RegisterSingleton<II2cBus, FakeII2cBus>();
            _container.RegisterSingleton<ISerialDevice, FakeISerialDevice>();
            _container.RegisterSingleton<IGpioDevice, FakeGpioDevice>();
        }

        protected override void RegisterConfiguration()
        {
            _container.RegisterInstance(new StartupConfiguration { ConfigurationLocation = @"..\..\..\componentConfiguration.json" });
        }
    }

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