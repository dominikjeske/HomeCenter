using System;
using System.Collections.Generic;
using System.Net;
using Amazon.Lambda.Core;
using Newtonsoft.Json;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]
namespace Wirehome.Alexa.OAuth
{
    public class Function
    {
        public object FunctionHandler(HttpRequest request, ILambdaContext context)
        {
            try
            {
                if (request.HttpMethod == "GET")
                {
                    var headers = new Dictionary<string, string>();
                    var redirectUrl = request.QueryStringParameters["redirect_uri"] + "?state=" +
                                      request.QueryStringParameters["state"] + "&code=123456";
                    headers.Add(HttpResponseHeader.Location.ToString(), redirectUrl);
                    return new LambdaResponse
                    {
                        Body = "",
                        StatusCode = HttpStatusCode.Redirect,
                        Headers = headers
                    };
                }

                return new LambdaResponse
                {
                    StatusCode = HttpStatusCode.OK,
                    Body = JsonConvert.SerializeObject(new AccessTokenResponse { AccessToken = "Dminik", TokenType = "bearer" })
                };
            }
            catch (Exception ex)
            {
                LambdaLogger.Log(ex.ToString());
            }
            return request;
        }
    }

    public class LambdaResponse
    {
        [JsonProperty("statusCode")]
        public HttpStatusCode StatusCode { get; set; }
        [JsonProperty("headers")]
        public Dictionary<string, string> Headers { get; set; }
        [JsonProperty("body")]
        public string Body { get; set; }
    }

    public class AccessTokenResponse
    {
        [JsonProperty("access_token")]
        public string AccessToken { get; set; }

        [JsonProperty("token_type")]
        public string TokenType { get; set; }

        [JsonProperty("expires_in")]
        public int ExpiresIn { get; set; }

        [JsonProperty("refresh_token")]
        public string RefreshToken { get; set; }
    }

    public class Headers
    {
        [JsonProperty("Accept")]
        public string Accept { get; set; }

        [JsonProperty("Accept-Encoding")]
        public string AcceptEncoding { get; set; }

        [JsonProperty("Accept-Language")]
        public string AcceptLanguage { get; set; }

        [JsonProperty("CloudFront-Forwarded-Proto")]
        public string CloudFrontForwardedProto { get; set; }

        [JsonProperty("CloudFront-Is-Desktop-Viewer")]
        public string CloudFrontIsDesktopViewer { get; set; }

        [JsonProperty("CloudFront-Is-Mobile-Viewer")]
        public string CloudFrontIsMobileViewer { get; set; }

        [JsonProperty("CloudFront-Is-SmartTV-Viewer")]
        public string CloudFrontIsSmartTVViewer { get; set; }

        [JsonProperty("CloudFront-Is-Tablet-Viewer")]
        public string CloudFrontIsTabletViewer { get; set; }

        [JsonProperty("CloudFront-Viewer-Country")]
        public string CloudFrontViewerCountry { get; set; }

        [JsonProperty("dnt")]
        public string Dnt { get; set; }

        [JsonProperty("Host")]
        public string Host { get; set; }

        [JsonProperty("User-Agent")]
        public string UserAgent { get; set; }

        [JsonProperty("Via")]
        public string Via { get; set; }

        [JsonProperty("X-Amz-Cf-Id")]
        public string XAmzCfId { get; set; }

        [JsonProperty("X-Amzn-Trace-Id")]
        public string XAmznTraceId { get; set; }

        [JsonProperty("X-Forwarded-For")]
        public string XForwardedFor { get; set; }

        [JsonProperty("X-Forwarded-Port")]
        public string XForwardedPort { get; set; }

        [JsonProperty("X-Forwarded-Proto")]
        public string XForwardedProto { get; set; }
    }

    public class Identity
    {
        [JsonProperty("cognitoIdentityPoolId")]
        public object CognitoIdentityPoolId { get; set; }

        [JsonProperty("accountId")]
        public object AccountId { get; set; }

        [JsonProperty("cognitoIdentityId")]
        public object CognitoIdentityId { get; set; }

        [JsonProperty("caller")]
        public object Caller { get; set; }

        [JsonProperty("sourceIp")]
        public string SourceIp { get; set; }

        [JsonProperty("accessKey")]
        public object AccessKey { get; set; }

        [JsonProperty("cognitoAuthenticationType")]
        public object CognitoAuthenticationType { get; set; }

        [JsonProperty("cognitoAuthenticationProvider")]
        public object CognitoAuthenticationProvider { get; set; }

        [JsonProperty("userArn")]
        public object UserArn { get; set; }

        [JsonProperty("userAgent")]
        public string UserAgent { get; set; }

        [JsonProperty("user")]
        public object User { get; set; }
    }

    public class RequestContext
    {
        [JsonProperty("requestTime")]
        public string RequestTime { get; set; }

        [JsonProperty("path")]
        public string Path { get; set; }

        [JsonProperty("accountId")]
        public string AccountId { get; set; }

        [JsonProperty("protocol")]
        public string Protocol { get; set; }

        [JsonProperty("resourceId")]
        public string ResourceId { get; set; }

        [JsonProperty("stage")]
        public string Stage { get; set; }

        [JsonProperty("requestTimeEpoch")]
        public long RequestTimeEpoch { get; set; }

        [JsonProperty("requestId")]
        public string RequestId { get; set; }

        [JsonProperty("identity")]
        public Identity Identity { get; set; }

        [JsonProperty("resourcePath")]
        public string ResourcePath { get; set; }

        [JsonProperty("httpMethod")]
        public string HttpMethod { get; set; }

        [JsonProperty("apiId")]
        public string ApiId { get; set; }
    }

    public class HttpRequest
    {
        [JsonProperty("resource")]
        public string Resource { get; set; }

        [JsonProperty("path")]
        public string Path { get; set; }

        [JsonProperty("httpMethod")]
        public string HttpMethod { get; set; }

        [JsonProperty("headers")]
        public Headers Headers { get; set; }

        [JsonProperty("queryStringParameters")]
        public Dictionary<string, string> QueryStringParameters { get; set; }

        [JsonProperty("pathParameters")]
        public object PathParameters { get; set; }

        [JsonProperty("stageVariables")]
        public object StageVariables { get; set; }

        [JsonProperty("requestContext")]
        public RequestContext RequestContext { get; set; }

        [JsonProperty("body")]
        public object Body { get; set; }

        [JsonProperty("isBase64Encoded")]
        public bool IsBase64Encoded { get; set; }
    }
}
