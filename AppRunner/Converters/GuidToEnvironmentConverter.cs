using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using AppRunner.Models;
using AppRunner.Services;
using EleCho.WpfSuite.ValueConverters;
using Microsoft.Extensions.DependencyInjection;

namespace AppRunner.Converters
{
    class GuidToEnvironmentConverter : SingletonValueConverterBase<GuidToEnvironmentConverter>
    {
        private readonly ConfigurationService _configurationService = 
            App.Services.GetRequiredService<ConfigurationService>();

        public override object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is Guid guid)
            {
                return _configurationService.Configuration.Environments?.FirstOrDefault(e => e.Guid == guid);
            }

            return null;
        }

        public override object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is RunEnvironment env)
            {
                return env.Guid;
            }

            return default(Guid);
        }
    }
}
