using System.Collections.Generic;
using System.Net;

namespace HomeCenter.Model.Messages.Queries.Services
{
    public abstract class HttpPostQuery : Query
    {
        protected HttpPostQuery()
        {
            RequestType = "POST";
        }

        public virtual object Parse(string rawHttpResult) => rawHttpResult;

        public CookieContainer Cookies { get; protected set; }

        public string Address
        {
            get => AsString(MessageProperties.Address);
            set => SetProperty(MessageProperties.Address, value);
        }

        public string Body
        {
            get => AsString(MessageProperties.Body);
            set => SetProperty(MessageProperties.Body, value);
        }

        public string RequestType
        {
            get => AsString(MessageProperties.RequestType);
            set => SetProperty(MessageProperties.RequestType, value);
        }

        public string ContentType
        {
            get => AsString(MessageProperties.ContentType);
            set => SetProperty(MessageProperties.ContentType, value);
        }

        public bool IgnoreReturnStatus
        {
            get => AsBool(MessageProperties.IgnoreReturnStatus);
            set => SetProperty(MessageProperties.IgnoreReturnStatus, value);
        }

        public IDictionary<string, string> AuthorisationHeader
        {
            get => AsDictionary(MessageProperties.AuthorisationHeader);
            set => SetProperty(MessageProperties.AuthorisationHeader, value);
        }

        public IDictionary<string, string> Headers
        {
            get => AsDictionary(MessageProperties.Headers);
            set => SetProperty(MessageProperties.Headers, value);
        }

        public IDictionary<string, string> Creditionals
        {
            get => AsDictionary(MessageProperties.Creditionals);
            set => SetProperty(MessageProperties.Creditionals, value);
        }
    }
}