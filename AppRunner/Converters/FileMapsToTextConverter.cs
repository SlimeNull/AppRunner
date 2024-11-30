using System.Globalization;
using AppRunner.Data;
using EleCho.WpfSuite.ValueConverters;

namespace AppRunner.Converters
{
    class FileMapsToTextConverter : SingletonValueConverterBase<FileMapsToTextConverter, List<ReferenceKeyValuePair<string, string>>, string>
    {
        public override List<ReferenceKeyValuePair<string, string>>? DefaultSourceValue => new();
        public override string? DefaultTargetValue => string.Empty;

        public override string? Convert(List<ReferenceKeyValuePair<string, string>> value, Type targetType, object? parameter, CultureInfo culture)
        {
            return string.Join(Environment.NewLine, value.Where(p => p is not null).Select(p => !string.IsNullOrWhiteSpace(p.Key) || !string.IsNullOrWhiteSpace(p.Value) ? $"{p.Key}>{p.Value}" : string.Empty));
        }

        public override List<ReferenceKeyValuePair<string, string>>? ConvertBack(string value, Type targetType, object? parameter, CultureInfo culture)
        {
            var result = new List<ReferenceKeyValuePair<string, string>>();

            foreach (var line in value.Split(Environment.NewLine))
            {
                if (string.IsNullOrWhiteSpace(line))
                {
                    result.Add(new ReferenceKeyValuePair<string, string>(string.Empty, string.Empty));
                    continue;
                }

                var segments = line.Split('>', 2);
                if (segments.Length == 1)
                {
                    result.Add(new ReferenceKeyValuePair<string, string>(segments[0], string.Empty));
                }
                else
                {
                    result.Add(new ReferenceKeyValuePair<string, string>(segments[0], segments[1]));
                }
            }

            return result;
        }
    }
}
