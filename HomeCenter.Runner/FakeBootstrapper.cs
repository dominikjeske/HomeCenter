using HomeCenter.Model.Contracts;
using HomeCenter.Services.Bootstrapper;
using HomeCenter.Services.Controllers;
using HomeCenter.Storage.RavenDB;
using HomeCenter.Utils.LogProviders;
using Microsoft.Extensions.Logging;
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
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using HomeCenter.Abstractions;

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
            return new ILoggerProvider[] { new ConsoleLogProvider() };
        }

        public static IDocumentStore EnsureExists(IDocumentStore store)
        {
            try
            {
                using var dbSession = store.OpenSession();
                dbSession.Query<LogEntry>().Take(0).ToList();
            }
            catch (DatabaseDoesNotExistException)
            {
                store.Maintenance.Server.Send(new CreateDatabaseOperation(new DatabaseRecord
                {
                    DatabaseName = store.Database
                }));
            }

            return store;
        }

        //TODO https://stackoverflow.com/questions/41243485/simple-injector-register-iloggert-by-using-iloggerfactory-createloggert

        protected override void ConfigureLoggerFactory(ILoggerFactory loggerFacory)
        {
            base.ConfigureLoggerFactory(loggerFacory);

            var docStore = CreateRavenDocStore();

            //var config = new ConfigurationBuilder()
            //.AddJsonFile("logging.json")
            //.Build();

            var logger = new LoggerConfiguration().Enrich.WithExceptionDetails(new DestructuringOptionsBuilder().WithDefaultDestructurers()
                                                  .WithFilter(new IgnorePropertyByNameExceptionFilter(nameof(Exception.HResult))))
                                                  //.ReadFrom.Configuration(config)
                                                  .WriteTo.Console()
                                                  .WriteTo.RavenDB(docStore)
                                                  .CreateLogger();

            loggerFacory.AddSerilog(logger);
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

            //TODO fix
            //docStore.Conventions.CustomizeJsonSerializer = s => s.TypeNameHandling = TypeNameHandling.None;
            docStore.Initialize();
            EnsureExists(docStore);

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
}