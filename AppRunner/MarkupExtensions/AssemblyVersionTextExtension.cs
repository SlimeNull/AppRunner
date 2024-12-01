using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Markup;

namespace AppRunner.MarkupExtensions
{
    public class AssemblyVersionTextExtension : MarkupExtension
    {
        public Type? Type { get; set; }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            if (Type is not null)
            {
                return (Type.Assembly.GetName().Version ?? new Version(1, 0, 0, 0)).ToString();
            }

            return (Assembly.GetExecutingAssembly().GetName().Version ?? new Version(1, 0, 0, 0)).ToString();
        }
    }
}
