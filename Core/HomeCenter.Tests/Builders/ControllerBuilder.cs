using HomeCenter.Model.Core;
using HomeCenter.Model.Messages.Commands.Service;
using HomeCenter.Model.Messages.Events.Service;
using HomeCenter.Model.Messages.Queries;
using HomeCenter.Tests.Dummies;
using HomeCenter.Tests.Helpers;
using HomeCenter.Tests.Mocks;
using Proto;
using SimpleInjector;
using System;
using System.Threading.Tasks;

namespace HomeCenter.Tests.ComponentModel
{
    public class ControllerBuilder 
    {
        private string _configuration;
        private string _adapter;
        private readonly Container _container = new Container();
        private PID _controller;
        private IMessageBroker _broker;

        public ControllerBuilder WithConfiguration(string configuration)
        {
            _configuration = configuration;
            return this;
        }

        public ControllerBuilder WithAdapter(string adapter)
        {
            _adapter = adapter;
            return this;
        }

        public async Task<(IMessageBroker broker, LogMock logs, ITestAdapter adapter)> BuildAndRun()
        {
            var bootstrapper = new MockBootstrapper(_container, _configuration);
            _controller = await bootstrapper.BuildController().ConfigureAwait(false);

            //var tcs = TaskHelper.GenerateTimeoutTaskSource<bool>(500);
            var tcs = new TaskCompletionSource<bool>();
            _broker = _container.GetInstance<IMessageBroker>();
            _broker.SubscribeForEvent<SystemStartedEvent>(_ => tcs.SetResult(true));
            await tcs.Task;

            var adapter = await _broker.Request<GetAdapterQuery, ITestAdapter>(new GetAdapterQuery(), _adapter).ConfigureAwait(false);

            return (_broker, bootstrapper.Logs, adapter);
        }

        public async Task Stop()
        {
            await _broker.Request<StopSystemQuery, bool>(StopSystemQuery.Default, _controller);
        }

        
    }
}