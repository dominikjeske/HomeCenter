using System.Collections.Generic;

namespace HomeCenter.Services.MotionService.Tests
{
    internal class DetectorDescription
    {
        public string DetectorName { get; set; }
        public List<string> Neighbors { get; set; } = new List<string>();

        public Dictionary<string, string> Properties = new Dictionary<string, string>();
    }
}