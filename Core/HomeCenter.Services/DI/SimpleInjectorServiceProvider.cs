using SimpleInjector;
using System;

namespace HomeCenter.Services.DI
{
    public class SimpleInjectorServiceProvider : IServiceProvider
    {
        private readonly Container _container;

        public SimpleInjectorServiceProvider(Container container)
        {
            _container = container;
        }

        public object GetService(Type serviceType) => _container.GetInstance(serviceType);
    }
}