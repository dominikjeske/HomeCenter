namespace Wirehome.Core.Interface.Native
{
    //
    // Summary:
    //     Describes whether the data transfers that the ReadPartial, WritePartial, or WriteReadPartial
    //     method performed succeeded, or provides the reason that the transfers did not
    //     succeed.
    public enum NativeI2cTransferStatus
    {
        //
        // Summary:
        //     The data was entirely transferred. For WriteReadPartial, the data for both the
        //     write and the read operations was entirely transferred.
        FullTransfer = 0,
        //
        // Summary:
        //     The I<sup>2</sup> C device negatively acknowledged the data transfer before all
        //     of the data was transferred.
        PartialTransfer = 1,
        //
        // Summary:
        //     The bus address was not acknowledged.
        SlaveAddressNotAcknowledged = 2,
        //
        // Summary:
        //     The transfer failed due to the clock being stretched for too long. Ensure the
        //     clock line is not being held low.
        ClockStretchTimeout = 3,
        //
        // Summary:
        //     The transfer failed for an unknown reason.
        UnknownError = 4
    }
}