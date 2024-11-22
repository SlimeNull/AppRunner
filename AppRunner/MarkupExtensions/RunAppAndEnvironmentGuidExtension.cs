using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Markup;
using AppRunner.Models;

namespace AppRunner.MarkupExtensions
{
    public class RunAppAndEnvironmentGuidExtension : MarkupExtension
    {
        public RunApp? App { get; set; }
        public RunEnvironment? Environment { get; set; }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return new RunAppAndEnvironmentGuid(App, Environment?.Guid ?? default);
        }
    }
}
