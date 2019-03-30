using HomeCenter.Model.Core;
using HomeCenter.Model.Messages.Queries;
using HomeCenter.Services.Controllers;
using HomeCenter.Tests.Dummies;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SimpleInjector;
using System.Threading.Tasks;

[assembly: Parallelize(Workers = 0, Scope = ExecutionScope.ClassLevel)]

namespace HomeCenter.Tests.ComponentModel
{
    public class BaseTest
    {
        protected Container Container;
        protected IMessageBroker Broker => Container.GetInstance<IMessageBroker>();
        protected LogMock Logs => Container.GetInstance<ILoggerProvider>() as LogMock;

        protected async Task<T> GetAdapter<T>(string adapterName = null)
        {
            if(adapterName == null)
            {
                adapterName = typeof(T).Name;
            }

            var result = await Broker.Request<GetAdapterQuery, T>(new GetAdapterQuery(), adapterName).ConfigureAwait(false);
            return result;
        }

        [TestCleanup]
        public async Task CleanUp()
        {
            await Broker.Request<StopSystemQuery, bool>(StopSystemQuery.Default, nameof(Controller)).ConfigureAwait(false);
            await Task.Delay(10);   //TODO timing required for safe close of previous test
        }

        [TestInitialize]
        public void Init()
        {
            Container = new Container();
        }
    }
}