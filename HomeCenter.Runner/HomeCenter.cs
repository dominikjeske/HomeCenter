using System;
using System.Device.Gpio.Drivers;
using System.Device.Gpio;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System.Threading;
using HomeCenter.Extensions;
using System.Runtime.InteropServices;

namespace HomeCenter.Runner
{
    internal class HomeCenter : IDisposable
    {
        private const int PIN_NUMBER = 21; 
        private readonly GpioController _controller;
        private readonly GpioPin _interruptPin;

        private readonly ILogger<HomeCenterRunner> _logger;
        private bool _disposedValue;

        public HomeCenter(ILogger<HomeCenterRunner> logger)
        {
            _logger = logger;            
            _logger.LogInformation("HomeCenter started");

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return;
            }

            _controller = new GpioController(PinNumberingScheme.Logical, new RaspberryPi3Driver());
            _interruptPin = _controller.OpenPin(PIN_NUMBER, PinMode.InputPullDown);
            _interruptPin.ValueChanged += _interruptPin_ValueChanged;
        }

        private void _interruptPin_ValueChanged(object sender, PinValueChangedEventArgs pinValueChangedEventArgs)
        {
            _logger.LogInformation($"Pin '{pinValueChangedEventArgs.PinNumber}' changed to {pinValueChangedEventArgs.ChangeType}");
        }

        public Task Run(CancellationToken cancellationToken)
        {
            return cancellationToken.AsTask();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _interruptPin.ValueChanged -= _interruptPin_ValueChanged;
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
