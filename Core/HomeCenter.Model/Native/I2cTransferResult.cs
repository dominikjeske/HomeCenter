namespace HomeCenter.Core.Interface.Native
{
    public struct I2cTransferResult
    {
        public I2cTransferStatus Status;
        public uint BytesTransferred;
    }
}