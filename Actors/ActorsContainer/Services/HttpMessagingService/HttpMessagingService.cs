using HomeCenter.CodeGeneration;
using HomeCenter.Model.Actors;
using HomeCenter.Model.Core;
using HomeCenter.Model.Messages;
using HomeCenter.Model.Messages.Queries.Services;
using HomeCenter.Utils.Extensions;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace HomeCenter.Services.Networking
{
    [ProxyCodeGenerator]
    public class HttpMessagingService : Service
    {
        [Subscribe(true)]
        protected async Task<object> SendGetRequest(HttpGetQuery httpMessage)
        {
            using (var httpClient = new HttpClient())
            {
                var httpResponse = await httpClient.GetAsync(httpMessage.Address).ConfigureAwait(false);
                httpResponse.EnsureSuccessStatusCode();
                var responseBody = await httpResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
                return httpMessage.Parse(responseBody);
            }
        }

        [Subscribe(true)]
        protected async Task<object> SendPostRequest(HttpPostQuery httpMessage)
        {
            //TODO Assert messages required properties

            var httpClientHandler = new HttpClientHandler();
            if (httpMessage.Cookies != null)
            {
                httpClientHandler.CookieContainer = httpMessage.Cookies;
                httpClientHandler.UseCookies = true;
            }

            if (httpMessage.ContainsProperty(MessageProperties.Creditionals))
            {
                var creditionals = httpMessage.Creditionals.First();
                httpClientHandler.Credentials = new System.Net.NetworkCredential(creditionals.Key, creditionals.Value);
            }

            using (var httpClient = new HttpClient(httpClientHandler))
            {
                if (httpMessage.ContainsProperty(MessageProperties.Headers))
                {
                    foreach (var header in httpMessage.Headers)
                    {
                        httpClient.DefaultRequestHeaders.Add(header.Key, header.Value);
                    }
                }
                if (httpMessage.ContainsProperty(MessageProperties.AuthorisationHeader))
                {
                    var header = httpMessage.AuthorisationHeader.First();
                    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(header.Key, header.Value);
                }

                var content = new StringContent(httpMessage.Body);
                if (httpMessage.ContainsProperty(MessageProperties.ContentType))
                {
                    content.Headers.ContentType = new MediaTypeHeaderValue(httpMessage.ContentType);
                }
                var response = await httpClient.PostAsync(httpMessage.Address, content).ConfigureAwait(false);

                if (!httpMessage.IgnoreReturnStatus)
                {
                    response.EnsureSuccessStatusCode();
                }

                var responseBody = await response.Content.ReadAsStringAsync(Encoding.UTF8).ConfigureAwait(false);
                return httpMessage.Parse(responseBody);
            }
        }
    }
}