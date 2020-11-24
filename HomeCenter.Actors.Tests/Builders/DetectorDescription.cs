using System.Collections.Generic;

namespace HomeCenter.Actors.Tests.Builders
{
    internal class DetectorDescription
    {
        public string DetectorName { get; set; } = string.Empty;
        public List<string> Neighbors { get; set; } = new List<string>();

        public Dictionary<string, string> Properties = new Dictionary<string, string>();
    }
}