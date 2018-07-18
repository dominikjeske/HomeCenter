using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NLog.Web;
using Microsoft.AspNetCore.Http;
using NLog.Extensions.Logging;
using System.Net;
using Microsoft.AspNetCore.Diagnostics;
using NSwag.AspNetCore;
using System.Reflection;
using Wirehome.WindowsService.Core;
using Wirehome.WindowsService.Services;

namespace Wirehome.WindowsService
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();
            Configuration = builder.Build();

            env.ConfigureNLog("nlog.config");
        }

        public IConfigurationRoot Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddSingleton<IAudioService, AudioService>();
            services.AddSingleton<IPowerService, PowerService>();
            services.AddSingleton<IProcessService, ProcessService>();
            services.AddSingleton<IDisplayService, DisplayService>();
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            //loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            //loggerFactory.AddDebug();

            loggerFactory.AddNLog();

            app.UseSwaggerUi(typeof(Startup).GetTypeInfo().Assembly); //TODO new SwaggerUiSettings<());

            app.UseExceptionHandler(options =>
            {
                 options.Run(
                 async context =>
                 {
                     context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                     context.Response.ContentType = "text/html";
                     var ex = context.Features.Get<IExceptionHandlerFeature>();
                     if (ex != null)
                     {
                         var err = $"<h1>Error: {ex.Error.Message}</h1>{ex.Error.StackTrace }";
                         await context.Response.WriteAsync(err).ConfigureAwait(false);
                     }

                     var logger = loggerFactory.CreateLogger("GlobalExceptionHandler");
                     logger.LogError(ex.Error, "Unhandled Excteption");
                 });
             }
            );

            app.UseMvc();
        }
    }
}
