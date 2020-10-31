using Scriban;

namespace HomeCenter.SourceGenerators
{
    internal static class TemplateGenerator
    {
        public static string Execute(string templateString, object model)
        {
            var template = Template.Parse(templateString);
            var result = template.Render(model, memberRenamer: member => member.Name);
            return result;
        }
    }
}