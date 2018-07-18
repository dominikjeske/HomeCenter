namespace Wirehome.Core.Services.I2C
{
    public interface II2CBusService : IService
    {
        II2CTransferResult Write(I2CSlaveAddress address, byte[] buffer, bool useCache = true);

        II2CTransferResult Read(I2CSlaveAddress address, byte[] buffer, bool useCache = true);

        II2CTransferResult WriteRead(I2CSlaveAddress address, byte[] writeBuffer, byte[] readBuffer, bool useCache = true);
    }
}