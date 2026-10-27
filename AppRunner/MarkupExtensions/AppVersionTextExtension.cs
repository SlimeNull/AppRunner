using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Markup;

namespace AppRunner.MarkupExtensions
{
    public class AppVersionTextExtension : MarkupExtension
    {
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            var v = typeof(AppVersionTextExtension).Assembly.GetName().Version;
            return v?.ToString() ?? "1.0.0.0";
        }
    }
}
