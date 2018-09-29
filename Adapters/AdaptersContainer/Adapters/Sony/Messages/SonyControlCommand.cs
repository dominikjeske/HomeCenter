using HomeCenter.Model.Core;
using HomeCenter.Model.Messages.Commands.Service;
using System;
using System.Net;

namespace HomeCenter.Model.Adapters.Sony
{
    public class SonyControlCommand : HttpCommand, IFormatableMessage<SonyControlCommand>
    {
        public string Code { get; set; }
        public string AuthorisationKey { get; set; }

        public SonyControlCommand FormatMessage()
        {
            DefaultHeaders.Add("SOAPACTION", "\"urn:schemas-sony-com:service:IRCC:1#X_SendIRCC\"");

            Cookies = new CookieContainer();
            Cookies.Add(new Uri($"http://{Address}/sony/"), new Cookie("auth", AuthorisationKey, "/sony", Address));
            Address = $"http://{Address}/sony/IRCC";
            Body =  $@"<?xml version=""1.0""?>
                    <s:Envelope xmlns:s=""http://schemas.xmlsoap.org/soap/envelope/"" s:encodingStyle=""http://schemas.xmlsoap.org/soap/encoding/"">
                        <s:Body>
                        <u:X_SendIRCC xmlns:u=""urn:schemas-sony-com:service:IRCC:1"">
                            <IRCCCode>{Code}</IRCCCode>
                        </u:X_SendIRCC>
                        </s:Body>
                    </s:Envelope>";

            return this;
        }
    }
}