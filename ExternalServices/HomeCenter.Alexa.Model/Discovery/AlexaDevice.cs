﻿using HomeCenter.Alexa.Model.Common;
using System.Collections.Generic;

namespace HomeCenter.Alexa.Model.Discovery
{
    public class AlexaDevice
    {
        public string Uid { get; set; }
        public string FriendlyName { get; set; }
        public string Description { get; set; }
        public string Room { get; set; }
        public List<DeviceCapability> Capabilities { get; set; }
    }

    public class DeviceCapability
    {
        public InterfaceType Interface { get; set; }
        public List<string> States { get; set; }
    }
}