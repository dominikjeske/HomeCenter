using HomeCenter.Assemblies;
using Proto;
using System;
using System.Linq;

namespace HomeCenter.Model.Extensions
{
    public static class ContainerExtensions
    {
        public static IActor GetActorProxy(this IServiceProvider container, Type actorType)
        {
            //TODO make it better - not only by Proxy name

            var proxyType = AssemblyHelper.GetAllTypes(actorType).Where(t => t.Name == $"{actorType.Name}Proxy").Single();
            var proxy = container.GetService(proxyType);

            if(proxy is null || proxy is not IActor)
            {
                throw new InvalidOperationException($"Cannot create instance of type {actorType.Name}");
            }

            return (IActor)proxy;
        }
    }
}