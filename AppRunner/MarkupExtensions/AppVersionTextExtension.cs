using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Markup;
using Windows.ApplicationModel;

namespace AppRunner.MarkupExtensions
{
    public class AppVersionTextExtension : MarkupExtension
    {
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return Package.Current.Id.Version.ToString() ?? "1.0.0.0";
        }
    }
}
