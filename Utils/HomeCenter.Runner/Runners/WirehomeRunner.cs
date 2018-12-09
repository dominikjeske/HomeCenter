using HomeCenter.Broker;
using HomeCenter.Model.Messages.Events.Service;
using SimpleInjector;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HomeCenter.Runner
{
    public class WirehomeRunner : Runner
    {
        private readonly List<Runner> _runners = new List<Runner>();

        public WirehomeRunner(List<Runner> runners) : base(nameof(WirehomeRunner), runners.Select(r => r.GetType().Name).ToArray())
        {
            _runners = runners;
        }

        public override async Task Run()
        {
            var tcs = new TaskCompletionSource<bool>();

            var container = new Container();
            var bootstrapper = new WirehomeBootstrapper(container, $@"..\..\..\..\..\Configurations\componentConfiguration.json");
            var controller = await bootstrapper.BuildController().ConfigureAwait(false);

            foreach (var runner in _runners)
            {
                runner.SetContainer(container);
            }

            var eventAggregator = container.GetInstance<IEventAggregator>();
            eventAggregator.Subscribe<SystemStartedEvent>(async message =>
            {
                await Task.Delay(500).ConfigureAwait(false);
                tcs.SetResult(true);
            });

            await tcs.Task.ConfigureAwait(false);
            await base.Run().ConfigureAwait(false);
        }

        public override Task RunTask(int taskId)
        {
            return _runners[taskId].Run();
        }
    }
}