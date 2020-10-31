namespace HomeCenter.SourceGenerators
{
    internal static class ParameterDescriptorExtensions
    {
        public static ParameterDescriptor ToCamelCase(this PropertyAssignDescriptor parameterDescriptor) => new ParameterDescriptor
        {
            Name = parameterDescriptor.Source.ToCamelCase(),
            Type = parameterDescriptor.Type
        };
    }
}