using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json;
using HomeCenter.Abstractions;
using HomeCenter.Messages.Queries.Services;

namespace HomeCenter.Adapters.Sony.Messages
{
    public class SonyRegisterQuery : HttpPostQuery, IFormatableMessage<SonyRegisterQuery>
    {
        public string ClientID { get; set; } = "";
        public string ApplicationID { get; set; } = "HomeCenter";
        public string PIN { get; set; }

        private Uri _cookieAddress;

        public SonyRegisterQuery FormatMessage()
        {
            if (string.IsNullOrWhiteSpace(Address))
            {
                throw new ArgumentException("Address cannot be empty");
            }

            if (!string.IsNullOrWhiteSpace(PIN))
            {
                AuthorisationHeader = new Dictionary<string, string>() { ["Basic"] = Convert.ToBase64String(new UTF8Encoding().GetBytes(":" + PIN)) };
            }
            else
            {
                IgnoreReturnStatus = true;
            }

            Cookies = new CookieContainer();
            _cookieAddress = new Uri($"http://{Address}/sony/");
            Cookies.Add(_cookieAddress, new Cookie("auth", "", "/sony", Address));

            Address = $"http://{Address}/sony/accessControl";
            Body = JsonSerializer.Serialize(new
            {
                @method = "actRegister",
                @params = new object[] { new ActRegisterRequest(ClientID, ApplicationID, "private"), new[] { new ActRegister1Request("WOL", "yes") } },
                @id = 1,
                @version = "1.0",
            });

            return this;
        }

        public string ReadAuthKey()
        {
            return Cookies.GetCookies(_cookieAddress)
                          .OfType<Cookie>()
                          .First(x => x.Name == "auth")
                          .Value;
        }

        public override object Parse(string rawHttpResult)
        {
            if (!string.IsNullOrWhiteSpace(PIN))
            {
                return ReadAuthKey();
            }

            return string.Empty;
        }
    }
}