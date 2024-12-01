using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Markup;

namespace AppRunner.MarkupExtensions
{
    public class EnumValuesExtension : MarkupExtension
    {
        public Type? Type { get; set; }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            if (Type == null)
            {
                throw new InvalidOperationException();
            }

            if (!Type.IsEnum)
            {
                throw new ArgumentException();
            }

            return Enum.GetValues(Type);
        }
    }
}
