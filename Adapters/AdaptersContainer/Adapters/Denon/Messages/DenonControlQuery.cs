using HomeCenter.Model.Messages.Queries.Services;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace HomeCenter.Model.Adapters.Denon
{
    internal class DenonControlQuery : HttpQuery
    {
        public string Command { get; set; }
        public string Api { get; set; }
        public string ReturnNode { get; set; }
        public string Zone { get; set; }

        public override string Address
        {
            get => MessageAddress();
            set => base.Address = value;
        }

        private string MessageAddress() => $"http://{Address}/goform/{Api}.xml?{Zone}{(Zone?.Length == 0 ? "" : "+")}{Command}";

        public string ParseResult(string responseData)
        {
            using (var reader = new StringReader(responseData))
            {
                if (responseData?.Length == 0 && ReturnNode?.Length == 0) return "";

                var xml = XDocument.Load(reader);
                var returnNode = xml.Descendants(ReturnNode).FirstOrDefault();
                return returnNode.Value;
            }
        }
    }
}