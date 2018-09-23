using HomeCenter.Core.Utils;
using HomeCenter.Model.Core;
using Proto;
using SimpleInjector;
using System;
using System.Linq;

namespace HomeCenter.Model.Extensions
{
    public static class ContainerExtensions
    {
        public static Registration RegisterService<T, K>(this Container container) where T : class, IService
                                                                                   where K : class, T
        {
            var registration = Lifestyle.Singleton.CreateRegistration<K>(container);
            container.AddRegistration<T>(registration);
            return registration;
        }

        public static Registration RegisterService<T>(this Container container, T instance) where T : class, IService
        {
            var registration = Lifestyle.Singleton.CreateRegistration<T>(() => instance, container);
            container.AddRegistration<T>(registration);
            return registration;
        }

        public static T GetActorProxy<T>(this IServiceProvider container) where T : class, IActor
        {
            var proxyType = AssemblyHelper.GetAllTypes<T>().Single();
            var proxy = container.GetService(proxyType);
            return proxy as T;
        }

        public static IActor GetActorProxy(this IServiceProvider container, Type actorType)
        {
            var proxyType = AssemblyHelper.GetAllTypes(actorType).Single();
            var proxy = container.GetService(proxyType);
            return proxy as IActor;
        }
    }
}