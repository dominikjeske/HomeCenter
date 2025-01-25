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
            _runners =
            [
               // new MqqtRunner("MQQT"),
               // new CCToolsLampRunner(new CCToolsAdapter(loggerFactory,"HSRel8_1", 24, true, false)),
               // new CCToolsLampRunner(new CCToolsAdapter(loggerFactory,"HSRel8_2", 1, 32, true, false)),

                //new CCToolsLampRunner(new CCToolsAdapter(loggerFactory,"HSPE16InputOnly_1", 1, 16, false, false)),
                new CCToolsLampRunner(new CCToolsAdapter(loggerFactory,"HSPE16InputOnly_2", 1, 88, false, false)),
                //new DimmerRunner("DimmerComponent")
            ];

            _tasks = _runners.Select(x => x.Uid).ToArray();
        }

        public override Task RunTask(int taskId)
        {
            return _runners[taskId].Run();
        }
    }
}