using System;
using System.Net;
using Wirehome.Core.Interface.Messaging;

namespace Wirehome.ComponentModel.Adapters.Sony
{
    public class SonyControlMessage : HttpMessage
    {
        public string Code { get; set; }
        public string AuthorisationKey { get; set; }

        public SonyControlMessage()
        {
            RequestType = "POST";
            DefaultHeaders.Add("SOAPACTION", "\"urn:schemas-sony-com:service:IRCC:1#X_SendIRCC\"");
        }

        public override string MessageAddress()
        {
            Cookies = new CookieContainer();
            Cookies.Add(new Uri($"http://{Address}/sony/"), new Cookie("auth", AuthorisationKey, "/sony", Address));
            return $"http://{Address}/sony/IRCC";
        }

        public override string Serialize()
        {
            return $@"<?xml version=""1.0""?>
                    <s:Envelope xmlns:s=""http://schemas.xmlsoap.org/soap/envelope/"" s:encodingStyle=""http://schemas.xmlsoap.org/soap/encoding/"">
                        <s:Body>
                        <u:X_SendIRCC xmlns:u=""urn:schemas-sony-com:service:IRCC:1"">
                            <IRCCCode>{Code}</IRCCCode>
                        </u:X_SendIRCC>
                        </s:Body>
                    </s:Envelope>";
        }

        public override object ParseResult(string responseData, Type responseType = null)
        {
            return string.Empty;

            //< s:Envelope xmlns:s = "http://schemas.xmlsoap.org/soap/envelope/" s: encodingStyle = "http://schemas.xmlsoap.org/soap/encoding/" xmlns: xsi = "http://www.w3.org/2001/XMLSchema-instance/" xmlns: xsd = "http://www.w3.org/2001/XMLSchema" >
            //  < s:Body >
            //       < u:X_SendIRCCResponse xmlns:u = "urn:schemas-sony-com:service:IRCC:1" >
            //        </ u:X_SendIRCCResponse >
            //     </ s:Body >
            //  </ s:Envelope >
        }
    }
}