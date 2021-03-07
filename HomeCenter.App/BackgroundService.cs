using HomeCenter.Abstractions;
using HomeCenter.Actors.Controllers;
using HomeCenter.Quartz;
using Microsoft.Extensions.Hosting;
using Proto;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace HomeCenter.App
{
    internal class BackgroundService : IHostedService, IDisposable
    {
        private readonly IActorFactory _actorFactory;
        private readonly QuartzInitializer _quartzInitializer;

        //TODO generalize initializers?
        public BackgroundService(IActorFactory actorFactory, QuartzInitializer quartzInitializer)
        {
            _actorFactory = actorFactory;
            _quartzInitializer = quartzInitializer;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            //TODO start actor
            await _quartzInitializer.Initialize();
            CreateController();
        }

        private PID CreateController()
        {
            return _actorFactory.CreateActor<RootActor>(nameof(RootActor));
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public void Dispose()
        {
        }
    }
}



//public static IDocumentStore EnsureExists(IDocumentStore store)
//{
//    try
//    {
//        using var dbSession = store.OpenSession();
//        dbSession.Query<LogEntry>().Take(0).ToList();
//    }
//    catch (DatabaseDoesNotExistException)
//    {
//        store.Maintenance.Server.Send(new CreateDatabaseOperation(new DatabaseRecord
//        {
//            DatabaseName = store.Database
//        }));
//    }

//    return store;
//}

////TODO https://stackoverflow.com/questions/41243485/simple-injector-register-iloggert-by-using-iloggerfactory-createloggert

//protected override void ConfigureLoggerFactory(ILoggerFactory loggerFacory)
//{
//    base.ConfigureLoggerFactory(loggerFacory);

//    var docStore = CreateRavenDocStore();

//    //var config = new ConfigurationBuilder()
//    //.AddJsonFile("logging.json")
//    //.Build();

//    var logger = new LoggerConfiguration().Enrich.WithExceptionDetails(new DestructuringOptionsBuilder().WithDefaultDestructurers()
//                                          .WithFilter(new IgnorePropertyByNameExceptionFilter(nameof(Exception.HResult))))
//                                          //.ReadFrom.Configuration(config)
//                                          .WriteTo.Console()
//                                          .WriteTo.RavenDB(docStore)
//                                          .CreateLogger();

//    loggerFacory.AddSerilog(logger);
//}

//private IDocumentStore CreateRavenDocStore()
//{
//    var docStore = new DocumentStore
//    {
//        Urls = new[]
//        {
//                    "http://127.0.0.1:8080"
//                },
//        Database = "Logs",
//        Conventions = { }
//    };

//    //TODO fix
//    //docStore.Conventions.CustomizeJsonSerializer = s => s.TypeNameHandling = TypeNameHandling.None;
//    docStore.Initialize();
//    EnsureExists(docStore);

//    //TESTS
//    using (var dbSession = docStore.OpenSession())
//    {
//        //var test = new Dictionary<string, string>() { ["imie"] = "Dominik", ["nazwisko"] = "Jeske" };
//        //dbSession.Store(test, "Move/");

//        //var meta = dbSession.Advanced.GetMetadataFor(test);
//        //meta["@collection"] = "Move";

//        //var a = new LogMessage<TestClass>();
//        //a.Value = testEntry;
//        //dbSession.Store(a, "Move/");

//        dbSession.SaveChanges();
//    }

//    return docStore;
//}

//var batchingOptions = new PeriodicBatchingSinkOptions
//{
//    BatchSizeLimit = 100,
//    Period = TimeSpan.FromMilliseconds(1),
//    EagerlyEmitFirstEvent = true,
//    QueueLimit = 10000
//};