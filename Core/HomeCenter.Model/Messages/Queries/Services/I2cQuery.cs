namespace HomeCenter.Model.Messages.Queries.Services
{
    public class I2cQuery : Query
    {
        public int Address { get; set; }
        public int Size { get; set; }
        public bool UseCache { get; set; }

        public static I2cQuery Create(int address, int size, bool useCache = true) => new I2cQuery
        {
            Address = address,
            Size = size,
            UseCache = useCache
        };
    }
}