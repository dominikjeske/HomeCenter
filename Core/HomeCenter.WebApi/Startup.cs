using System;
using HomeCenter.Messages;
using HomeCenter.WebApi.Controllers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Proto.Remote;

namespace HomeCenter.WebApi
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
            services.Configure<WireHomeConfigSection>(Configuration.GetSection("WireHome"));
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            
            app.UseMvc();

            StartProtoActor();
        }

        private void StartProtoActor()
        {
            Serialization.RegisterFileDescriptor(ProtoMessagesReflection.Descriptor);
            Remote.Start("127.0.0.1", 0);
        }
    }
}