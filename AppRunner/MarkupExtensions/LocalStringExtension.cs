using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Shapes;

namespace AppRunner.MarkupExtensions
{
    public class LocalStringExtension : MarkupExtension
    {
        public string? Name { get; set; }

        public LocalStringExtension()
        {

        }

        public LocalStringExtension(string name)
        {
            Name = name;
        }

        public override object? ProvideValue(IServiceProvider serviceProvider)
        {
            if (string.IsNullOrWhiteSpace(Name))
            {
                return Name;
            }

            return Resources.Strings.ResourceManager.GetObject(Name) as string;
        }
    }
}
