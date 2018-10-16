using HomeCenter.Model.Messages;
using HomeCenter.Model.Messages.Queries.Services;
using HomeCenter.Utils.Extensions;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace HomeCenter.Adapters.Denon.Messages
{
    internal class DenonStatusLightQuery : HttpQuery, IFormatableMessage<DenonStatusLightQuery>
    {
        public string Zone { get; set; } = "1";

        private float? NormalizeVolume(float? volume) => volume == null ? null : volume + 80.0f;

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

        public DenonStatusLightQuery FormatMessage()
        {
            if (Zone == "1")
            {
                Address = $"http://{Address}/goform/formMainZone_MainZoneXmlStatusLite.xml";
            }
            else
            {
                Address = $"http://{Address}/goform/formZone{Zone}_Zone{Zone}XmlStatusLite.xml";
            }

            return this;
        }
    }
}