using HomeCenter.Model.Messages;
using HomeCenter.Model.Messages.Queries.Services;
using System.Linq;
using System.Xml.Linq;

namespace HomeCenter.Adapters.Denon.Messages
{
    internal class DenonStatusQuery : HttpGetQuery, IFormatableMessage<DenonStatusQuery>
    {
        public DenonStatusQuery FormatMessage()
        {
            Address = $"http://{Address}/goform/formMainZone_MainZoneXmlStatus.xml";

            return this;
        }

        public override object Parse(string rawHttpResult)
        {
            var xml = XDocument.Parse(rawHttpResult);

            var renamed = xml.Descendants("InputFuncList").Descendants("value").Select(x => x.Value.Trim()).ToList();
            var input = xml.Descendants("RenameSource").Descendants("value").Descendants("value").Select(x => x.Value.Trim()).ToList();

            return new DenonDeviceInfo
            {
                InputSources = input.Zip(renamed, (k, v) => new { k, v }).ToDictionary(x => x.k, x => x.v),
                Surround = xml.Descendants("SurrMode").FirstOrDefault()?.Value?.Trim(),
                Model = xml.Descendants("Model").FirstOrDefault()?.Value?.Trim()
            };
        }
    }
}