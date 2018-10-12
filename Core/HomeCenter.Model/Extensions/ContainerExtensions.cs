using HomeCenter.Model.Core;
using HomeCenter.Utils;
using Proto;
using SimpleInjector;
using System;
using System.Linq;

namespace HomeCenter.Model.Extensions
{
    public static class ContainerExtensions
    {
        public static Registration RegisterService<T, K>(this Container container) where T : Service
                                                                                   where K : class, T
        {
            var registration = Lifestyle.Singleton.CreateRegistration<K>(container);
            container.AddRegistration<T>(registration);
            return registration;
        }

        public static Registration RegisterService<T>(this Container container, T instance) where T : Service
        {
            var registration = Lifestyle.Singleton.CreateRegistration<T>(() => instance, container);
            container.AddRegistration<T>(registration);
            return registration;
        }

        public static T GetActorProxy<T>(this IServiceProvider container) where T : class, IActor
        {
            return GetActorProxy(container, typeof(T)) as T;
        }

        public static IActor GetActorProxy(this IServiceProvider container, Type actorType)
        {
            var proxyType = AssemblyHelper.GetAllTypes(actorType).Single();

            var proxy = container.GetService(proxyType);
            return proxy as IActor;
        }
    }
}