using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Wirehome.Alexa.Model.Common;

namespace Wirehome.Alexa.Model.Discovery
{
    public class DiscoverResponse
    {
        [JsonProperty("event")]
        public Event Event { get; set; }

        public static DiscoverResponse GenerateResponse(IList<AlexaDevice> devices)
        {
            var endpoints = new List<Endpoint>();
            foreach(var device in devices)
            {
                endpoints.Add(new Endpoint
                {
                    EndpointId = device.Uid,
                    FriendlyName = device.FriendlyName,
                    ManufacturerName = "Wirehome",
                    Description = device.Description,
                    DisplayCategories = GetDisplayCategory(device),
                    Cookie = new Cookie { ExtraDetail1 = device.Room },
                    Capabilities = device.Capabilities.Select(capability =>
                    new Capability
                    {
                        Interface = $"Alexa.{capability.Interface.ToString()}",
                        ProactivelyReported = true,
                        SupportsDeactivation = true,
                        Retrievable = true,
                        Properties = new Properties
                        {
                            Supported = capability.States.Select(state => new Supported { Name = state }).ToList()
                        }
                    }).ToList()
            });
            }

            return new DiscoverResponse
            {
                Event = new Event
                {
                    Header = new Header
                    {
                        MessageId = Guid.NewGuid().ToString(),
                        Namespace = "Alexa.Discovery",
                        Name = "Discover.Response"
                    },
                    Payload = new DiscoveryResponsePayload
                    {
                        Endpoints = endpoints
                    }
                }
            };
        }

        public static IList<string> GetDisplayCategory(AlexaDevice device)
        {
            var list = new List<string>();
            //TODO fix the logic
            if(device.Capabilities.Any(capability => capability.Interface == InterfaceType.PowerController))
            {
                list.Add(nameof(DisplayCategory.LIGHT));
            }

            if(list.Count == 0)
            {
                list.Add(nameof(DisplayCategory.OTHER));
            }

            return list;
        }
    }

    public class Event
    {
        [JsonProperty("header")]
        public Header Header { get; set; }

        [JsonProperty("payload")]
        public DiscoveryResponsePayload Payload { get; set; }
    }

    public class DiscoveryResponsePayload
    {
        [JsonProperty("endpoints")]
        public IList<Endpoint> Endpoints { get; set; }
    }

    public class Endpoint
    {
        [JsonProperty("endpointId")]
        public string EndpointId { get; set; }

        [JsonProperty("friendlyName")]
        public string FriendlyName { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("manufacturerName")]
        public string ManufacturerName { get; set; }

        [JsonProperty("displayCategories")]
        public IList<string> DisplayCategories { get; set; }

        [JsonProperty("cookie")]
        public Cookie Cookie { get; set; }

        [JsonProperty("capabilities")]
        public IList<Capability> Capabilities { get; set; }
    }

    public class Cookie
    {
        [JsonProperty("extraDetail1")]
        public string ExtraDetail1 { get; set; }

        [JsonProperty("extraDetail2")]
        public string ExtraDetail2 { get; set; }

        [JsonProperty("extraDetail3")]
        public string ExtraDetail3 { get; set; }

        [JsonProperty("extraDetail4")]
        public string ExtraDetail4 { get; set; }
    }

    public class Capability
    {
        [JsonProperty("type")]
        public string Type { get; set; } = "AlexaInterface";

        [JsonProperty("interface")]
        public string Interface { get; set; }

        [JsonProperty("version")]
        public string Version { get; set; } = "3";

        [JsonProperty("properties")]
        public Properties Properties { get; set; }

        [JsonProperty("supportsDeactivation")]
        public bool? SupportsDeactivation { get; set; }

        [JsonProperty("proactivelyReported")]
        public bool? ProactivelyReported { get; set; }

        [JsonProperty("retrievable")]
        public bool? Retrievable { get; set; }

        [JsonProperty("cameraStreamConfigurations")]
        public IList<CameraStreamConfiguration> CameraStreamConfigurations { get; set; }
    }

    public class Properties
    {
        [JsonProperty("supported")]
        public IList<Supported> Supported { get; set; }

        [JsonProperty("proactivelyReported")]
        public bool ProactivelyReported { get; set; }

        [JsonProperty("retrievable")]
        public bool Retrievable { get; set; }
    }

    public class Supported
    {
        [JsonProperty("name")]
        public string Name { get; set; }
    }

    public class Resolution
    {
        [JsonProperty("width")]
        public int Width { get; set; }

        [JsonProperty("height")]
        public int Height { get; set; }
    }

    public class CameraStreamConfiguration
    {
        [JsonProperty("protocols")]
        public IList<string> Protocols { get; set; }

        [JsonProperty("resolutions")]
        public IList<Resolution> Resolutions { get; set; }

        [JsonProperty("authorizationTypes")]
        public IList<string> AuthorizationTypes { get; set; }

        [JsonProperty("videoCodecs")]
        public IList<string> VideoCodecs { get; set; }

        [JsonProperty("audioCodecs")]
        public IList<string> AudioCodecs { get; set; }
    }
}
