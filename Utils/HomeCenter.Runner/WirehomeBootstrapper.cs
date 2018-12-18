using HomeCenter.Model.Contracts;
using HomeCenter.Services.Bootstrapper;
using HomeCenter.Services.Controllers;
using SimpleInjector;
using System;
using System.Reactive.Disposables;

namespace HomeCenter.Runner
{
    public class WirehomeBootstrapper : Bootstrapper
    {
        public WirehomeBootstrapper(Container container) : base(container)
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
            _container.RegisterInstance(new StartupConfiguration { ConfigurationLocation = @"..\..\..\..\..\Configurations\componentConfiguration.json" });
        }
    }

    public class FakeII2cBus : II2cBus
    {
        public void Dispose()
        {
            
        }

        public Span<byte> Read(int address)
        {
            return Span<byte>.Empty;
        }

        public void Write(int address, Span<byte> data)
        {
        }
    }

    public class FakeGpioDevice : IGpioDevice
    {
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