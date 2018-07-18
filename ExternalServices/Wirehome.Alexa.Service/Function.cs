using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Amazon.Lambda.Core;
using Newtonsoft.Json;
using Wirehome.Alexa.Model.Common;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace Wirehome.Alexa.Service
{
    public class Function
    {
        public string HandlerUri { get; set; }

        public async Task<object> FunctionHandler(object request, ILambdaContext context)
        {
            try
            {
                using (var httpClient = new HttpClient())
                {
                    var response = await httpClient.PostAsync(HandlerUri, new StringContent(request.ToString(), Encoding.UTF8, "application/json")).ConfigureAwait(false);

                    if(response.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        return await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                    }
                }
            }
            catch (Exception ex)
            {
                LambdaLogger.Log(ex.ToString());
            }
            return null;
        }
    }
}
