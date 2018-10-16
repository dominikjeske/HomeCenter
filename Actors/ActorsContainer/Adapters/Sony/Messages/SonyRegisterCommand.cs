using HomeCenter.Model.Messages;
using HomeCenter.Model.Messages.Commands.Service;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace HomeCenter.Adapters.Sony.Messages
{
    public class SonyRegisterCommand : HttpCommand, IFormatableMessage<SonyRegisterCommand>
    {
        public string ClientID { get; set; } = "faed787d-8cac-4b67-8c0d-4e291584843b";
        public string ApplicationID { get; set; } = "HomeCenter";
        public string PIN { get; set; }

        public string ReadAuthKey()
        {
            return Cookies.GetCookies(new Uri($"http://{Address}/sony/"))
                          .OfType<Cookie>()
                          .First(x => x.Name == "auth")
                          .Value;
        }

        public SonyRegisterCommand FormatMessage()
        {
            if (string.IsNullOrWhiteSpace(Address))
            {
                throw new ArgumentException("Address cannot be empty");
            }

            if (!string.IsNullOrWhiteSpace(PIN))
            {
                AuthorisationHeader = new KeyValuePair<string, string>("Basic", Convert.ToBase64String(new UTF8Encoding().GetBytes(":" + PIN)));
            }

            Cookies = new CookieContainer();
            Cookies.Add(new Uri($"http://{Address}/sony/"), new Cookie("auth", "", "/sony", Address));

            Address = $"http://{Address}/sony/accessControl";
            Body = JsonConvert.SerializeObject(new
            {
                @method = "actRegister",
                @params = new object[] { new ActRegisterRequest(ClientID, ApplicationID, "private"), new[] { new ActRegister1Request("WOL", "yes") } },
                @id = 1,
                @version = "1.0",
            });

            return this;
        }
    }
}