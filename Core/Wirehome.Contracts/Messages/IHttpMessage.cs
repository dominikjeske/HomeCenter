using System.Collections.Generic;
using System.Net;
using System;

namespace Wirehome.Core.Interface.Messaging
{
    public interface IHttpMessage 
    {
        string Address { get; set; }
        KeyValuePair<string, string> AuthorisationHeader { get; set; }
        string ContentType { get; set; }
        CookieContainer Cookies { get; set; }
        NetworkCredential Creditionals { get; set; }
        Dictionary<string, string> DefaultHeaders { get; set; }
        string RequestType { get; set; }

        string MessageAddress();

        string Serialize();

        object ParseResult(string responseData, Type responseType = null);
    }
}