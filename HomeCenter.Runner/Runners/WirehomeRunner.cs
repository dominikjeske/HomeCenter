using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace HomeCenter.Runner
{
    public class HomeCenterRunner : Runner
    {
        private readonly List<Runner> _runners = new List<Runner>();

        public HomeCenterRunner() : base(nameof(HomeCenterRunner))
        {
            _runners = new List<Runner>
            {
                new DenonRunner("DenonComponent"),
                new KodiRunner("KodiComponent"),
                new PcRunner("PcComponent"),
                new SamsungRunner("SamsungComponent"),
                new SonyRunner("SonyComponent"),
                new RemoteSocketRunner("RemoteLamp3"),
                new RaspberryRunner("RaspberryLed"),
                new CCToolsLampRunner("CCToolsLamp"),
                new DimmerRunner("DimmerComponent")
            };

            _tasks = _runners.Select(x => x.Uid).ToArray();
        }

        public override async Task Run()
        {
            var tcs = new TaskCompletionSource<bool>();
            var container = new Container();

            foreach (var runner in _runners)
            {
                runner.SetContainer(container);
            }
        }

        public override Task RunTask(int taskId)
        {
            return _runners[taskId].Run();
        }
    }
}