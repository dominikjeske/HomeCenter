using HomeCenter.Utils.LogProviders;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HomeCenter.Runner
{
    public class HomeCenterRunner : Runner
    {
        private readonly List<Runner> _runners = new List<Runner>();

        public HomeCenterRunner(ILoggerFactory loggerFactory) : base(nameof(HomeCenterRunner))
        {
            //var loggerProvider = new ConsoleLogProvider();
            
            _runners = new List<Runner>
            {
                new MqqtRunner("MQQT"),
                new CCToolsLampRunner("HSRel8_1", new CCToolsAdapter(loggerFactory.CreateLogger("HSRel8_1"), 24, true, false)),
                //new CCToolsLampRunner("HSRel8_2", new CCToolsAdapter(loggerProvider.CreateLogger("HSRel8_2"), i2cService, 32, true, false)),

                //new CCToolsLampRunner("HSPE16InputOnly_1", new CCToolsAdapter(loggerProvider.CreateLogger("HSPE16InputOnly_1"), i2cService, 16, false, false)),
                //new CCToolsLampRunner("HSPE16InputOnly_2", new CCToolsAdapter(loggerProvider.CreateLogger("HSPE16InputOnly_2"), i2cService, 88, false, false)),
                //new DimmerRunner("DimmerComponent")
            };

            _tasks = _runners.Select(x => x.Uid).ToArray();
        }

        public override Task RunTask(int taskId)
        {
            return _runners[taskId].Run();
        }
    }
}