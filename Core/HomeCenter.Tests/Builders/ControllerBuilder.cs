using HomeCenter.Model.Core;
using HomeCenter.Model.Messages.Events.Service;
using HomeCenter.Tests.Dummies;
using HomeCenter.Tests.Mocks;
using SimpleInjector;
using System.Threading.Tasks;

namespace HomeCenter.Tests.ComponentModel
{
    public class ControllerBuilder
    {
        private string _configuration;
        private readonly Container _container = new Container();

        public ControllerBuilder WithConfiguration(string configuration)
        {
            _configuration = configuration;
            return this;
        }

        public async Task<(TestAdapter adapter, IMessageBroker broker)> BuildAndRun()
        {
            var bootstrapper = new MockBootstrapper(_container, _configuration);
            var controller = await bootstrapper.BuildController().ConfigureAwait(false);

            //var tcs = TaskHelper.GenerateTimeoutTaskSource<bool>(1000);
            var tcs = new TaskCompletionSource<bool>();
            var broker = _container.GetInstance<IMessageBroker>();
            broker.SubscribeForEvent<SystemStartedEvent>(_ => tcs.SetResult(true));
            await tcs.Task;

            var adapter = await broker.Request<GetAdapterQuery, TestAdapter>(new GetAdapterQuery(), "TestAdapter");

            return (adapter, broker);
        }
    }
}