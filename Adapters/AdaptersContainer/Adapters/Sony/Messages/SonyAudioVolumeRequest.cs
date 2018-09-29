using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Net;
using HomeCenter.Model.Messages.Commands.Service;
using HomeCenter.Model.Core;

namespace HomeCenter.Model.Adapters.Sony
{

    public class SonyAudioVolumeRequest
    {
        [JsonProperty("target")]
        public string Target { get; set; }

        [JsonProperty("volume")]
        public string Volume { get; set; }

        public SonyAudioVolumeRequest()
        {
        }

        public SonyAudioVolumeRequest(string @target, string @volume)
        {
            Target = @target;
            Volume = @volume;
        }
    }
}