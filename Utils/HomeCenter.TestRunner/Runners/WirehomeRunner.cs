using HomeCenter.Model.Core;
using SimpleInjector;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HomeCenter.TestRunner
{
    public class WirehomeRunner : Runner
    {
        private List<Runner> _Runners = new List<Runner>();

        public WirehomeRunner(List<Runner> runners) : base(nameof(WirehomeRunner), runners.Select(r => r.GetType().Name).ToArray())
        {
            _Runners = runners;
        }

        public async Task Initialize()
        {
            var container = new Container();
            var bootstrapper = new WirehomeBootstrapper(container, "componentConfiguration");
            var controller = await bootstrapper.BuildController().ConfigureAwait(false);

            foreach(var runner in _Runners)
            {
                runner.SetContainer(container);
            }

            await Task.Delay(1000).ConfigureAwait(false);
        }

        public override Task RunTask(int taskId)
        {
            return _Runners[taskId].Run();
        }
    }
}