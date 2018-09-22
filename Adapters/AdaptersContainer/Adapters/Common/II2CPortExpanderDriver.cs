namespace HomeCenter.Model.Adapters.Drivers
{
    public interface II2CPortExpanderDriver
    {
        int StateSize { get; }

        void Write(byte[] state);

        byte[] Read();
    }
}
