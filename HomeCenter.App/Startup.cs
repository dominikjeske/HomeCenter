using HomeCenter.Abstractions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HomeCenter.App
{
    public class Startup
    {
        private readonly IConfiguration _configuration;

        public Startup(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddOptions();
            services.AddControllers();
            services.AddSingleton<UnhandledExceptionHandler>();
            services.AddHostedService<BackgroundService>();
            services.Configure<HomeCenterOptions>(_configuration.GetSection("HomeCenter"));
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.RegisterUnhandledHandlers();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints => endpoints.MapControllers());
        }
    }
}