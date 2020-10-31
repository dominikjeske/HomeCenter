using HomeCenter.Abstractions;
using HomeCenter.Assemblies;
using HomeCenter.Configuration;
using HomeCenter.Model.Actors;
using HomeCenter.Services.Actors;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

[assembly: HostingStartup(typeof(ConfigurationHosting))]

namespace HomeCenter.Configuration
{
    public class ConfigurationHosting : IHostingStartup
    {
        public static string Assembly => System.Reflection.Assembly.GetAssembly(typeof(ConfigurationHosting)).GetName().Name;

        public void Configure(IWebHostBuilder builder)
        {
            builder.ConfigureServices(s =>
            {
                s.AddSingleton<ActorPropsRegistry>();
                s.AddSingleton<IActorFactory, ActorFactory>();
                s.AddSingleton<IActorLoader, ActorLoader>();
                s.AddSingleton<ClassActivator>();
                s.AddSingleton<BaseObjectMapper>();
                s.AddSingleton<DeviceActorMapper>();

                s.AddSingleton<ITypeMapper, ServiceMapper>();
                s.AddSingleton<ITypeMapper, AdapterMapper>();
                s.AddSingleton<ITypeMapper, AreaMapper>();
                s.AddSingleton<ITypeMapper, ComponentMapper>();

                foreach (var actorProxy in AssemblyHelper.GetTypesWithAttribute<ProxyClassAttribute>())
                {
                    if (actorProxy == typeof(ComponentProxy)) continue;

                    s.AddTransient(actorProxy);
                }
            });
        }
    }
}