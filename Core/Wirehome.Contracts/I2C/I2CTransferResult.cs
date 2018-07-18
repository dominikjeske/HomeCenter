namespace Wirehome.Core.Services.I2C
{
    public struct I2CTransferResult : II2CTransferResult
    {
        public I2CTransferResult(I2CTransferStatus status, int bytesTransferred)
        {
            Status = status;
            BytesTransferred = bytesTransferred;
        }

        public I2CTransferStatus Status { get; }
        public int BytesTransferred { get; }
    }
}