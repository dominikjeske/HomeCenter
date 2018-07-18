namespace Wirehome.Core.Interface.Native
{
    public enum NativeGpioSharingMode
    {
        //
        // Summary:
        //     Opens the GPIO pin exclusively, so that no other connection to the pin can be
        //     opened.
        Exclusive = 0,
        //
        // Summary:
        //     Opens the GPIO pin as shared, so that other connections in **SharedReadOnly**
        //     mode to the pin can be opened.
        SharedReadOnly = 1
    }
}