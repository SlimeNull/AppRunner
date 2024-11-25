using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using AppRunner.Models;

namespace AppRunner.Converters
{
    public class RunAppAndEnvironmentToAppAndEnvironmentGuidConverter : IMultiValueConverter
    {
        public static RunAppAndEnvironmentToAppAndEnvironmentGuidConverter Instance { get; } = new();

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values is not [RunApp app, RunEnvironment env])
            {
                return DependencyProperty.UnsetValue;
            }

            return new RunAppAndEnvironmentGuid(app, env.Guid);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
