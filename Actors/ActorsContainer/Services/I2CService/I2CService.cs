using HomeCenter.CodeGeneration;
using HomeCenter.Model.Actors;
using HomeCenter.Model.Core;
using HomeCenter.Model.Messages.Commands.Service;
using HomeCenter.Model.Messages.Queries.Services;
using HomeCenter.Model.Native;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace HomeCenter.Services.Networking
{
    [ProxyCodeGenerator]
    public abstract class I2CService : Service
    {
        private readonly II2cBus _nativeI2CBus;

        protected I2CService(II2cBus nativeI2CBus)
        {
            _nativeI2CBus = nativeI2CBus;
        }

        [Subscibe]
        protected async Task Handle(I2cCommand command)
        {
            var address = command.Address;
            var useCache = command.UseCache; //TODO
            var data = command.Body;

            CheckAddress(address);

            try
            {
                await _nativeI2CBus.Write(address, data).ConfigureAwait(false);
            }
            catch (Exception exception)
            {
                Logger.LogWarning(exception, $"Error while accessing I2C device with address {address}. {exception.Message}");
            }
        }

        protected Task<byte[]> Handle(I2cQuery query)
        {
            var address = query.Address;
            var useCache = query.UseCache;

            //try
            //{
            //    return _nativeI2CBus.Read(address, query.Size);
            //}
            //catch (Exception exception)
            //{
            //    Logger.LogWarning(exception, $"Error while accessing I2C device with address {address}.");
            //}

            return Task.FromResult(new byte[] { }); //TODO
        }

        private void CheckAddress(int value)
        {
            if (value < 0 || value > 127) throw new ArgumentOutOfRangeException(nameof(value), "I2C address is invalid.");
            if (value >= 0x00 && value <= 0x07) throw new ArgumentOutOfRangeException(nameof(value), "I2C address " + value + " is reserved.");
            if (value >= 0x78 && value <= 0x7f) throw new ArgumentOutOfRangeException(nameof(value), "I2C address " + value + " is reserved.");
        }
    }
}