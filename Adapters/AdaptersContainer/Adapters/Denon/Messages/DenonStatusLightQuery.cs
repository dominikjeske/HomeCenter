using HomeCenter.Core.Extensions;
using HomeCenter.Model.Messages.Queries.Services;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace HomeCenter.Model.Adapters.Denon
{
    internal class DenonStatusLightQuery : HttpQuery
    {
        public string Zone { get; set; } = "1";

        public override string Address
        {
            get => MessageAddress();
            set => base.Address = value;
        }

        private string MessageAddress()
        {
            if (Zone == "1")
            {
                return $"http://{Address}/goform/formMainZone_MainZoneXmlStatusLite.xml";
            }
            return $"http://{Address}/goform/formZone{Zone}_Zone{Zone}XmlStatusLite.xml";
        }

        private float? NormalizeVolume(float? volume)
        {
            return volume == null ? null : volume + 80.0f;
        }

        public DenonStatus ParseResult(string responseData)
        {
            using (var reader = new StringReader(responseData))
            {
                var xml = XDocument.Parse(responseData);

                return new DenonStatus
                {
                    ActiveInput = xml.Descendants("InputFuncSelect").FirstOrDefault()?.Value?.Trim(),
                    PowerStatus = xml.Descendants("Power").FirstOrDefault()?.Value?.Trim().ToLower() == "on" ? true : false,
                    MasterVolume = NormalizeVolume(xml.Descendants("MasterVolume").FirstOrDefault()?.Value?.Trim().ToFloat()),
                    Mute = xml.Descendants("Mute").FirstOrDefault()?.Value?.Trim().ToLower() == "on"
                };
            }
        }
    }
}