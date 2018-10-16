namespace HomeCenter.Adapters.Common
{
    public interface II2CPortExpanderDriver
    {
        int StateSize { get; }

        void Write(byte[] state);

        byte[] Read();
    }
}