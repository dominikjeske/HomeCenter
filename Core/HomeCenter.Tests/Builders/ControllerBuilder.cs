using HomeCenter.Model.Core;
using HomeCenter.Model.Messages.Events.Service;
using Proto;
using SimpleInjector;
using System;
using System.Threading;
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

        public async Task<PID> BuildAndRun(int numberOfComponents = 1)
        {
            var bootstrapper = new MockBootstrapper(_container, _configuration);
            _controller = await bootstrapper.BuildController();
            _broker = _container.GetInstance<IMessageBroker>();

            await WaitToStart(numberOfComponents);

            return _controller;
        }

        private async Task WaitToStart(int numberOfComponents = 1)
        {
            var m_currentCount = numberOfComponents;
            //var tcs = TaskHelper.GenerateTimeoutTaskSource<bool>(500);
            var tcs = new TaskCompletionSource<bool>();
            _broker.SubscribeForEvent<ComponentStartedEvent>(_ =>
            {
                if (m_currentCount > 1)
                {
                    Interlocked.Decrement(ref m_currentCount);
                }
                else
                {
                    tcs.SetResult(true);
                }
                }, "*");
            await tcs.Task;
        }
    }
}