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
    class GuidToEnvironmentConverter : SingletonValueConverterBase<GuidToEnvironmentConverter, Guid, RunEnvironment>
    {
        private readonly ConfigurationService _configurationService = 
            App.Services.GetRequiredService<ConfigurationService>();

        public override Guid DefaultSourceValue => default;
        public override RunEnvironment? DefaultTargetValue => null;

        public override RunEnvironment? Convert(Guid value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value == default(Guid))
            {
                return RunEnvironment.Empty;
            }

            return _configurationService.Configuration.Environments?.FirstOrDefault(e => e.Guid == value);
        }

        public override Guid ConvertBack(RunEnvironment value, Type targetType, object? parameter, CultureInfo culture)
        {
            return value.Guid;
        }
    }
}
