using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HomeCenter.TestRunner
{
    public class WirehomeRunner : Runner
    {
        private List<Runner> _Runners = new List<Runner>();

        public WirehomeRunner(List<Runner> runners) : base(runners.Select(r => r.GetType().Name).ToArray())
        {
            _Runners = runners;
        }

        public async Task Initialize()
        {
            var bootstrapper = new WirehomeBootstrapper("componentConfiguration");
            var controller = await bootstrapper.BuildController().ConfigureAwait(false);

            await Task.Delay(1000).ConfigureAwait(false);
        }

        public override Task RunTask(int taskId)
        {
            return _Runners[taskId].Run();
        }
    }
}