using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Device.I2c;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using HomeCenter.Extensions;
using Microsoft.Extensions.Logging;

namespace HomeCenter
{
    public class CCToolsAdapter : IDisposable
    {
        private readonly MAX7311Driver _driver = new MAX7311Driver();
        private readonly ILogger _logger;
        private int _poolDurationWarning;
        private int _i2cAddress;
        private bool _firstPortWriteMode;
        private bool _secondPortWriteMode;
        private I2cBus _bus;
        private I2cDevice _i2cDevice;
        private bool _disposedValue;

        public string Name { get; }

        public CCToolsAdapter(ILoggerFactory logger, string name, int i2cAddress, bool firstPortWriteMode, bool secondPortWriteMode, int poolDurationWarning = 2000)
        {
            _logger = logger.CreateLogger(name);
            Name = name;
            _poolDurationWarning = poolDurationWarning;
            _i2cAddress = i2cAddress;
            _firstPortWriteMode = firstPortWriteMode;
            _secondPortWriteMode = secondPortWriteMode;

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) 
            {
                return; 
            }

            _bus = I2cBus.Create(1);
            _i2cDevice = _bus.CreateDevice(i2cAddress);

            ConfigureDriver();
            FetchState();

            if (_firstPortWriteMode)
            {
                for (int i = 0; i < 8; i++)
                {
                    SetPortState(i, false);
                }
            }

            if (_secondPortWriteMode)
            {
                for (int i = 8; i < 15; i++)
                {
                    SetPortState(i, false);
                }
            }
        }

        private void ConfigureDriver()
        {
            _i2cDevice.Write(_driver.Configure(_firstPortWriteMode, _secondPortWriteMode));
        }

        public async Task TurnOn(int pinNumber, TimeSpan? autoTurnOffAfter)
        {
            pinNumber = ValidatePin(pinNumber);

            SetPortState(pinNumber, true);

            if (autoTurnOffAfter.HasValue)
            {
                await Task.Delay(autoTurnOffAfter.Value);
                SetPortState(pinNumber, false);
            }
        }

        public void TurnOff(int pinNumber)
        {
            pinNumber = ValidatePin(pinNumber);
            SetPortState(pinNumber, false);
        }

        public void Switch(int pinNumber)
        {
            pinNumber = ValidatePin(pinNumber);
            var currentState = _driver.GetState(pinNumber);

            SetPortState(pinNumber, !currentState);
        }

        public bool GetState(int pinNumber)
        {
            var state =  _driver.GetState(pinNumber);
            _logger.LogInformation($"Pin '{pinNumber}' state: {state}");

            return state;
        }

        private int ValidatePin(int pinNumber)
        {
            if (pinNumber < 0 || pinNumber > 15)
            {
                throw new ArgumentOutOfRangeException(nameof(pinNumber));
            }

            var isPinInFirstPortRange = pinNumber < 8;

            if ((isPinInFirstPortRange && !_firstPortWriteMode) || (!isPinInFirstPortRange && !_secondPortWriteMode))
            {
                throw new ArgumentException($"Pin {pinNumber} is configured for INPUT");
            }

            return pinNumber;
        }

        private void SetPortState(int pinNumber, bool state)
        {
            var newState = _driver.GenerateNewState(pinNumber, state);

            try
            {
                _i2cDevice.Write(newState);
                _driver.AcceptNewState();
            }
            catch (Exception)
            {
                _driver.RevertNewState();
                throw;
            }

            _logger.LogInformation("Board committed state '{state}'", _driver.GetState().ToBinaryString());
        }

        public IDictionary<int, bool> FetchState()
        {
            var stopwatch = Stopwatch.StartNew();
            var newState = ReadFromBus();
            stopwatch.Stop();

            if (!_driver.TrySaveState(newState, out var oldState))
            {
                return ImmutableDictionary<int, bool>.Empty;
            }
            var dictionary = new Dictionary<int, bool>();
            var oldStateBits = new BitArray(oldState);
            var newStateBits = new BitArray(newState);

            _logger.LogTrace("fetched different state [{oldState}->{newState}]", oldState.ToBinaryString(), newState.ToBinaryString());

            for (int pinNumber = 0; pinNumber < oldStateBits.Length; pinNumber++)
            {
                var oldPinState = oldStateBits.Get(pinNumber);
                var newPinState = newStateBits.Get(pinNumber);
                bool pinInWriteMode = IsPinInWriteMode(pinNumber);

                // When state is the same or change is in port that are set to WRITE we skip event generation
                if (oldPinState == newPinState || pinInWriteMode)
                {
                    continue;
                }

                dictionary.Add(pinNumber, newPinState);
                _logger.LogTrace("Pin [{pinNumber}] state changed {oldPinState}->{newPinState}", pinNumber, oldPinState, newPinState);
            }

            if (stopwatch.ElapsedMilliseconds > _poolDurationWarning)
            {
                _logger.LogWarning("Polling device took {elapsed}ms.", stopwatch.ElapsedMilliseconds);
            }

            return dictionary;
        }

        private bool IsPinInWriteMode(int pinNumber)
        {
            var isPinInFirstPortRange = pinNumber < 8;
            var pinInWriteMode = (isPinInFirstPortRange && _firstPortWriteMode) || (!isPinInFirstPortRange && _secondPortWriteMode);
            return pinInWriteMode;
        }

        private byte[] ReadFromBus()
        {
            _i2cDevice.Write(_driver.GetReadTable());
            var result = new byte[_driver.BufferSize];
            _i2cDevice.Read(result);

            return result;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _i2cDevice?.Dispose();
                    _bus?.Dispose();
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