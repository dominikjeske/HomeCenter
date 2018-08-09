namespace HomeCenter.Core.Interface.Native
{
    public interface II2cBus
    {
        II2cDevice CreateDevice(string deviceId, int slaveAddress);
        string GetBusId();
    }
}