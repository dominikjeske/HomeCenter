using HTTPnet;
using HTTPnet.Core;
using HTTPnet.Core.Http;
using HTTPnet.Core.Pipeline;
using HTTPnet.Core.Pipeline.Handlers;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using HomeCenter.Core.Services.Logging;

namespace HomeCenter.Services.Networking
{
    public class HttpServerService : IHttpServerService
    {
        private HttpServer _httpServer = null;
        private bool _IsInitialized = false;
        private readonly List<IHttpContextPipelineHandler> _handlers = new List<IHttpContextPipelineHandler>();
        private readonly ILogger _logger;
        private HttpServerOptions _httpServerOptions = HttpServerOptions.Default;

        public HttpServerService(ILogService logService)
        {
            _logger = logService.CreatePublisher(nameof(HttpServerService));
        }

        public void Dispose()
        {
            _httpServer?.Dispose();
        }

        public async Task Initialize()
        {
            var pipeline = new HttpContextPipeline(new HttpExceptionHandler(_logger));
            pipeline.Add(new RequestBodyHandler());
            pipeline.Add(new TraceHandler());
            pipeline.Add(new ResponseBodyLengthHandler());
            pipeline.Add(new ResponseCompressionHandler());
            foreach (var handler in _handlers)
            {
                pipeline.Add(handler);
            }

            _httpServer = new HttpServerFactory().CreateHttpServer();
            _httpServer.RequestHandler = pipeline;
            await _httpServer.StartAsync(_httpServerOptions).ConfigureAwait(false);
            _IsInitialized = true;
        }

        public class HttpExceptionHandler : IHttpContextPipelineExceptionHandler
        {
            private readonly ILogger _logger;

            public HttpExceptionHandler(ILogger logger)
            {
                _logger = logger;
            }

            public Task HandleExceptionAsync(HttpContext httpContext, Exception exception)
            {
                httpContext.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

                httpContext.Response.Body = new MemoryStream(Encoding.UTF8.GetBytes(exception.ToString()));
                httpContext.Response.Headers[HttpHeader.ContentLength] = httpContext.Response.Body.Length.ToString(CultureInfo.InvariantCulture);

                httpContext.CloseConnection = true;

                _logger.Error(exception, $"Exception in {nameof(HttpServerService)}");

                return Task.FromResult(0);
            }
        }

        public void AddRequestHandler(IHttpContextPipelineHandler handler)
        {
            if (_IsInitialized) throw new Exception($"Cannot modify {nameof(HttpServerService)} after initialization");
            _handlers.Add(handler);
        }

        public void UpdateServerPort(int port)
        {
            if (_IsInitialized) throw new Exception($"Cannot modify {nameof(HttpServerService)} after initialization");
            _httpServerOptions.Port = port;
        }
    }
}