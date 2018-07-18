namespace Wirehome.Core.Interface.Native
{
    //
    // Summary:
    //     Provides information about whether the data transfers that the ReadPartial, WritePartial,
    //     or WriteReadPartial method performed succeeded, and the actual number of bytes
    //     the method transferred.
    public struct NativeI2cTransferResult
    {
        //
        // Summary:
        //     An enumeration value that indicates if the read or write operation transferred
        //     the full number of bytes that the method requested, or the reason that the full
        //     transfer did not succeed. For WriteReadPartial, the value indicates whether the
        //     data for both the write and the read operations was entirely transferred.
        public NativeI2cTransferStatus Status;
        //
        // Summary:
        //     The actual number of bytes that the operation actually transferred. The following
        //     table describes what this value represents for each method.
        public uint BytesTransferred;
    }
}