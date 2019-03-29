using HomeCenter.Model.Core;
using HomeCenter.Model.Messages.Queries;
using HomeCenter.Services.Controllers;
using HomeCenter.Tests.Dummies;
using HomeCenter.Tests.Mocks;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SimpleInjector;
using System.Threading.Tasks;

namespace HomeCenter.Tests.ComponentModel
{
    public class BaseTest
    {
        protected Container Container;
        protected IMessageBroker Broker => Container.GetInstance<IMessageBroker>();
        protected LogMock Logs => Container.GetInstance<ILoggerProvider>() as LogMock;

        protected async Task<ITestAdapter> GetAdapter(string adapterName) => await Broker.Request<GetAdapterQuery, ITestAdapter>(new GetAdapterQuery(), adapterName).ConfigureAwait(false);

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