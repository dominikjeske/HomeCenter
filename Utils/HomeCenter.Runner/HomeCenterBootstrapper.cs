using HomeCenter.Model.Contracts;
using HomeCenter.Services.Bootstrapper;
using HomeCenter.Services.Controllers;
using SimpleInjector;
using System;
using System.Reactive.Disposables;
using System.Reactive.Linq;

namespace HomeCenter.Runner
{
    public class HomeCenterBootstrapper : Bootstrapper
    {
        public HomeCenterBootstrapper(Container container) : base(container)
        {
            _container.Options.AllowOverridingRegistrations = true;
        }

        public Container Container => _container;

        protected override void RegisterNativeServices()
        {
            _container.RegisterSingleton<II2cBus, FakeII2cBus>();
            _container.RegisterSingleton<ISerialDevice, FakeISerialDevice>();
            _container.RegisterSingleton<IGpioDevice, FakeGpioDevice>();
        }

        protected override void RegisterConfiguration()
        {
            _container.RegisterInstance(new StartupConfiguration { ConfigurationLocation = @"..\..\..\componentConfiguration.json" });
        }
    }

    public class FakeII2cBus : II2cBus
    {
        public void Read(int address, byte[] buffer)
        {
            
        }

        public void Write(int address, byte[] data)
        {
            
        }

        public void WriteRead(int deviceAddress, byte[] writeBuffer, byte[] readBuffer)
        {
            
        }
    }

    public class FakeGpioDevice : IGpioDevice
    {
        public IObservable<PinChanged> PinChanged => Observable.Empty<PinChanged>();

        public void Dispose()
        {
            
        }

        public void RegisterPinChanged(int pinNumber, string pinMode)
        {
            
        }

        public void Write(int pin, bool value)
        {
        }
    }

    public class FakeISerialDevice : ISerialDevice
    {
        public void Dispose()
        {
        }

        public void Init()
        {
        }

        public void Send(byte[] data)
        {
        }

        public void Send(string data)
        {
        }

        public IDisposable Subscribe(Action<byte[]> handler) => Disposable.Empty;
    }
}