namespace HomeCenter.Model.Native
{
    public interface II2cBus
    {
        II2cDevice CreateDevice(string deviceId, int slaveAddress);

        string GetBusId();
    }
}