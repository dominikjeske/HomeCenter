using System.Linq;
using System.Xml.Linq;
using System.IO;
using System;
using Wirehome.Core.Interface.Messaging;

namespace Wirehome.ComponentModel.Adapters.Denon
{
    public class DenonMappingMessage : HttpMessage
    {
        public DenonMappingMessage()
        {
            RequestType = "GET";
        }

        public override string MessageAddress()
        {
            return $"http://{Address}/goform/formMainZone_MainZoneXml.xml";
        }

        public override object ParseResult(string responseData, Type responseType = null)
        {
            using (var reader = new StringReader(responseData))
            {
                var xml = XDocument.Parse(responseData);

                return new DenonDeviceInfo
                {
                    FriendlyName = xml.Descendants("FriendlyName").FirstOrDefault()?.Value?.Trim(),
                    InputMap = xml.Descendants("VideoSelectLists").Descendants("value").ToDictionary(y => y.Attribute("table").Value, x => x.Attribute("index").Value)
                };
            }
        }
    }
}