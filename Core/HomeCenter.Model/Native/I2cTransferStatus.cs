namespace HomeCenter.Core.Interface.Native
{
    public enum I2cTransferStatus
    {
        FullTransfer = 0,
        PartialTransfer = 1,
        SlaveAddressNotAcknowledged = 2,
        ClockStretchTimeout = 3,
        UnknownError = 4
    }
}