using System.Linq;
using System.Xml.Linq;
using System.IO;
using System;
using Wirehome.Core.Interface.Messaging;

namespace Wirehome.ComponentModel.Adapters.Denon
{
    public class DenonControlMessage : HttpMessage
    {
        public string Command { get; set; }
        public string Api { get; set; }
        public string ReturnNode { get; set; }
        public string Zone { get; set; }

        public DenonControlMessage()
        {
            RequestType = "GET";
        }

        public override string MessageAddress() => $"http://{Address}/goform/{Api}.xml?{Zone}{(Zone?.Length == 0 ? "" : "+")}{Command}";

        public override object ParseResult(string responseData, Type responseType = null)
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