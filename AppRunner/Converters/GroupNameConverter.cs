using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AppRunner.Resources;
using EleCho.WpfSuite.ValueConverters;

namespace AppRunner.Converters
{
    public class GroupNameConverter : SingletonValueConverterBase<GroupNameConverter, string, string>
    {
        public override string? DefaultTargetValue => Strings.Common_Default;

        public override string Convert(string value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return Strings.Common_Default;
            }

            return value;
        }
    }
}
