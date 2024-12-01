using System.Windows.Markup;

namespace AppRunner.MarkupExtensions
{
    public class LicenseTextExtension : MarkupExtension
    {
        public string? Name { get; set; }

        public LicenseTextExtension(string name)
        {
            Name = name;
        }

        public override object? ProvideValue(IServiceProvider serviceProvider)
        {
            if (string.IsNullOrWhiteSpace(Name))
            {
                return null;
            }

            return Resources.Licenses.ResourceManager.GetObject(Name) as string;
        }
    }
}
