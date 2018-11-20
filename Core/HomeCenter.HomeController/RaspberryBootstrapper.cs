using HomeCenter.HomeController.NativeServices;
using HomeCenter.Model.Native;
using HomeCenter.Services.Bootstrapper;
using HomeCenter.Services.Controllers;
using SimpleInjector;

namespace HomeCenter.HomeController
{
    internal class RaspberryBootstrapper : Bootstrapper
    {
        internal RaspberryBootstrapper(Container container) : base(container)
        {
            _container.Options.AllowOverridingRegistrations = true;
        }

        protected override void RegisterNativeServices()
        {
            _container.RegisterSingleton<IGpioController, RaspberryGpioController>();
            _container.RegisterSingleton<II2cBus, RaspberryI2cBus>();
            _container.RegisterSingleton<ISerialDevice, RaspberrySerialDevice>();
            _container.RegisterSingleton<ISoundPlayer, RaspberrySoundPlayer>();
            _container.RegisterSingleton<IStorage, RaspberryStorage>();
        }

        protected override void RegisterControllerOptions()
        {
            _container.RegisterInstance(new ControllerOptions
            {
                AdapterMode = "Embedded",
                Configuration = "componentConfiguration.json"
            });
        }
    }
}