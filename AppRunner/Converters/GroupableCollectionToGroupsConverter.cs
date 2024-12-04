using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AppRunner.Abstraction;
using EleCho.WpfSuite.ValueConverters;

namespace AppRunner.Converters
{
    public class GroupableCollectionToGroupsConverter : SingletonValueConverterBase<GroupableCollectionToGroupsConverter, IEnumerable<IGroupable>, IEnumerable<string>>
    {
        public override IEnumerable<string>? DefaultTargetValue => Array.Empty<string>();

        public override IEnumerable<string>? Convert(IEnumerable<IGroupable> value, Type targetType, object? parameter, CultureInfo culture)
        {
            return value
                .Select(groupable => groupable.Group)
                .Distinct()
                .Where(group => !string.IsNullOrWhiteSpace(group));
        }
    }
}
