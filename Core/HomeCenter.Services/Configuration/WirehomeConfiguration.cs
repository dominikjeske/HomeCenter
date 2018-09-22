using HomeCenter.Model.Components;
using HomeCenter.Core.ComponentModel.Areas;
using Proto;
using System.Collections.Generic;

namespace HomeCenter.Model.Configuration
{
    public class HomeCenterConfiguration
    {
        public IList<Component> Components { get; set; }
        public IList<PID> Adapters { get; set; }
        public IList<Area> Areas { get; set; }
    }
}