using HomeCenter.Utils;
using Proto;
using System;
using System.Linq;

namespace HomeCenter.Model.Extensions
{
    public static class ContainerExtensions
    {
        public static IActor GetActorProxy(this IServiceProvider container, Type actorType)
        {
            var proxyType = AssemblyHelper.GetAllTypes(actorType).Single();
            var proxy = container.GetService(proxyType);
            return proxy as IActor;
        }
    }
}