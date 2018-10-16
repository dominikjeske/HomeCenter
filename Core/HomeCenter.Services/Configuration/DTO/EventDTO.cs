using HomeCenter.Model.Core;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace HomeCenter.Services.Configuration.DTO
{
    public class EventDTO : BaseDTO
    {
        [JsonProperty("SourceDeviceUid")]
        public string SourceDeviceUid { get; set; }
    }
}