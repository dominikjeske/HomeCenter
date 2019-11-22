using HomeCenter.CodeGeneration;
using System;

namespace HomeCenter.Model.Mapper
{
    public interface IMapper<Source, Destination>
    {
        Destination Map(Source source);
    }

    public abstract class BaseAdapter<Source, Destination>
    {
        protected virtual void Configure(IConfigureMapping<Source, Destination> config)
        {
        }
    }

    public class SimpleResolver : INameResolver
    {
        public string Resolve(string input) => input;
    }

    public class UnderscoreResolver : INameResolver
    {
        public string Resolve(string input) => input[1..^0];
    }

    public class IgnoreCaseComparer : IPropertiesComparer
    {
        public bool CanMap(string source, string destination) => string.Equals(source, destination, StringComparison.InvariantCultureIgnoreCase);
    }
}