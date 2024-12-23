using System;
using System.Collections.Generic;
using System.Device.Gpio.Drivers;
using System.Device.Gpio;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;
using Microsoft.Extensions.Logging;
using System.Threading;
using HomeCenter.Extensions;
using System.Runtime.InteropServices;

namespace HomeCenter.Runner
{
    internal class HomeCenter : IDisposable
    {
        private readonly GpioController _controller;
        private readonly GpioPin _interruptPin;

        private readonly ILogger<HomeCenterRunner> _logger;
        private bool _disposedValue;

        public HomeCenter(ILogger<HomeCenterRunner> logger)
        {
            int pinNumber = 21;

            _logger = logger;
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return;
            }

            _controller = new GpioController(PinNumberingScheme.Logical, new RaspberryPi3Driver());
            _interruptPin = _controller.OpenPin(pinNumber, PinMode.InputPullUp);

            _controller.RegisterCallbackForPinValueChangedEvent(pinNumber, PinEventTypes.Falling, PinFalling);
            _controller.RegisterCallbackForPinValueChangedEvent(pinNumber, PinEventTypes.Rising, PinRising);
        }

        public Task Run(CancellationToken cancellationToken)
        {
            return cancellationToken.AsTask();
        }


        private void PinFalling(object sender, PinValueChangedEventArgs pinValueChangedEventArgs)
        {
            _logger.LogInformation("Pin falling");
        }

        private void PinRising(object sender, PinValueChangedEventArgs pinValueChangedEventArgs)
        {
            _logger.LogInformation("Pin rising");
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _controller.UnregisterCallbackForPinValueChangedEvent(_interruptPin.PinNumber, PinFalling);
                    _controller.UnregisterCallbackForPinValueChangedEvent(_interruptPin.PinNumber, PinRising);

                    _controller.Dispose();
                }

                _disposedValue = true;
            }
        }
        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
