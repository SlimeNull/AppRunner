using System.Windows.Markup;
using AppRunner.Models;

namespace AppRunner.MarkupExtensions
{
    public class EmptyEnvironment : MarkupExtension
    {
        public override object ProvideValue(IServiceProvider serviceProvider) => RunEnvironment.Empty;
    }
}
