using HomeCenter.Model.Areas;
using Proto;
using System.Collections.Generic;

namespace HomeCenter.Services.Configuration
{
    public class HomeCenterConfiguration
    {
        public IDictionary<string, PID> Components { get; set; }
        public IDictionary<string, PID> Adapters { get; set; }
        public IDictionary<string, PID> Services { get; set; }
        public IList<Area> Areas { get; set; }

    }
}