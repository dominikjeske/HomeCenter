using HomeCenter.Model.Core;
using SimpleInjector;

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
    }
}