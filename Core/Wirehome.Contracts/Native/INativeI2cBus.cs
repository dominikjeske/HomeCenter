namespace Wirehome.Core.Interface.Native
{
    public interface INativeI2cBus
    {
        INativeI2cDevice CreateDevice(string deviceId, int slaveAddress);
        string GetBusId();
    }
}