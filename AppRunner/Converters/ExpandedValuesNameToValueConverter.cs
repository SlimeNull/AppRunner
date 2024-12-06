using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using EleCho.WpfSuite.ValueConverters;

namespace AppRunner.Converters
{
    class ExpandedValuesNameToValueConverter : SingletonValueConverterBase<ExpandedValuesNameToValueConverter, string, bool>
    {
        public IDictionary<string, bool> ExpandedValues
        {
            get { return (IDictionary<string, bool>)GetValue(ExpandedValuesProperty); }
            set { SetValue(ExpandedValuesProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ExpandedValues.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ExpandedValuesProperty =
            DependencyProperty.Register("ExpandedValues", typeof(IDictionary<string, bool>), typeof(ExpandedValuesNameToValueConverter), new PropertyMetadata(null));



        public override bool DefaultTargetValue => false;

        public override bool Convert(string key, Type targetType, object? parameter, CultureInfo culture)
        {
            if (ExpandedValues == null ||
                !ExpandedValues.TryGetValue(key, out var value))
            {
                return false;
            }

            return value;
        }
    }
}
