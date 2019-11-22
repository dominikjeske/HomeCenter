namespace HomeCenter.Model.Mapper
{
    public class Src
    {
        public string _name { get; set; }
        public int _age { get; set; }
        public int _ignoredSource { get; set; }

        public int? _defaultValuedProperty { get; set; }
    }

    public class Dst
    {
        public string Name { get; set; }
        public int Age { get; set; }
        public int IgnoredDestination { get; set; }
        public int DefaultValuedProperty { get; set; }

        public int CustomValue { get; set; }

        public int CustomValue2 { get; set; }
    }
}