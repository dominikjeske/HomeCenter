using System;
using System.Threading.Tasks;

namespace HomeCenter.TestRunner
{
    public class WirehomeRunner
    {
        public async Task Run()
        {
            Console.Clear();

            var bootstrapper = new WirehomeBootstrapper("componentConfiguration");
            var controller = await bootstrapper.BuildController().ConfigureAwait(false);
        }
    }
}