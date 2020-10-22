using System.Linq;
using System.Xml.Linq;
using HomeCenter.Abstractions;
using HomeCenter.Messages.Queries.Services;

namespace HomeCenter.Adapters.Denon.Messages
{
    internal class DenonMappingQuery : HttpGetQuery, IFormatableMessage<DenonMappingQuery>
    {
        public DenonMappingQuery FormatMessage()
        {
            Address = $"http://{Address}/goform/formMainZone_MainZoneXml.xml";

            return this;
        }

        public override object Parse(string rawHttpResult)
        {
            var xml = XDocument.Parse(rawHttpResult);

            return new DenonDeviceInfo
            {
                FriendlyName = xml.Descendants("FriendlyName").FirstOrDefault()?.Value?.Trim(),
                InputMap = xml.Descendants("VideoSelectLists").Descendants("value").ToDictionary(y => y.Attribute("table").Value, x => x.Attribute("index").Value)
            };
        }
    }
}