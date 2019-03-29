using HomeCenter.Model.Core;
using HomeCenter.Model.Messages.Events.Service;
using Proto;
using SimpleInjector;
using System;
using System.Threading.Tasks;

namespace HomeCenter.Tests.ComponentModel
{
    public class ControllerBuilder
    {
        private string _configuration;
        private readonly Container _container;
        private PID _controller;
        private IMessageBroker _broker;

        public ControllerBuilder(Container container)
        {
            _container = container;
        }

        public ControllerBuilder WithConfiguration(string configuration)
        {
            _configuration = configuration;
            return this;
        }

        public async Task<PID> BuildAndRun()
        {
            var bootstrapper = new MockBootstrapper(_container, _configuration);
            _controller = await bootstrapper.BuildController().ConfigureAwait(false);
            _broker = _container.GetInstance<IMessageBroker>();

            await WaitToStart();

            return _controller;
        }

        private async Task WaitToStart()
        {
            //var tcs = TaskHelper.GenerateTimeoutTaskSource<bool>(500);
            var tcs = new TaskCompletionSource<bool>();
            _broker.SubscribeForEvent<SystemStartedEvent>(_ => tcs.SetResult(true));
            await tcs.Task;
        }
    }
}