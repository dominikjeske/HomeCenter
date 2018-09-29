using HomeCenter.Model.Messages.Queries.Services;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace HomeCenter.Model.Adapters.Denon
{
    internal class DenonMappingQuery : HttpQuery
    {
        public override string Address
        {
            get => MessageAddress();
            set => base.Address = value;
        }

        private string MessageAddress()
        {
            return $"http://{Address}/goform/formMainZone_MainZoneXml.xml";
        }

        public DenonDeviceInfo ParseResult(string responseData)
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