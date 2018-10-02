using HomeCenter.Model.Areas;
using HomeCenter.Model.Components;
using Proto;
using System.Collections.Generic;

namespace HomeCenter.Services.Configuration
{
    public class HomeCenterConfiguration
    {
        public IList<Component> Components { get; set; }
        public IList<PID> Adapters { get; set; }
        public IList<Area> Areas { get; set; }
    }
}