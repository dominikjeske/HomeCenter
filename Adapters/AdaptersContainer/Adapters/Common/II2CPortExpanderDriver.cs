namespace HomeCenter.ComponentModel.Adapters.Drivers
{
    internal interface II2CPortExpanderDriver
    {
        int StateSize { get; }

        void Write(byte[] state);

        byte[] Read();
    }
}
