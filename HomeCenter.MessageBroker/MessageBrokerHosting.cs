using HomeCenter.Abstractions;
using HomeCenter.MessageBroker;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

[assembly: HostingStartup(typeof(MessageBrokerHosting))]

namespace HomeCenter.MessageBroker
{
    public class MessageBrokerHosting : IHostingStartup
    {
        public static string Assembly => System.Reflection.Assembly.GetAssembly(typeof(MessageBrokerHosting)).GetName().Name;

        public void Configure(IWebHostBuilder builder)
        {
            builder.ConfigureServices(s =>
            {
                s.AddSingleton<IMessageBroker, MessageBroker>();
                s.AddSingleton<IEventAggregator, EventAggregator>();
            });
        }
    }
}