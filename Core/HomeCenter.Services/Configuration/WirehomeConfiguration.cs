using System.Collections.Generic;
using HomeCenter.ComponentModel.Adapters;
using HomeCenter.ComponentModel.Components;
using HomeCenter.Core.ComponentModel.Areas;

namespace HomeCenter.ComponentModel.Configuration
{
    public class HomeCenterConfiguration
    {
        public IList<Component> Components { get; set; }
        public IList<Adapter> Adapters { get; set; }
        public IList<Area> Areas { get; set; }
    }
}