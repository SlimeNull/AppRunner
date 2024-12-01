using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;

namespace AppRunner.Models
{
    public record class AppNavigationItem(string Icon, string Title, Type TargetPageType, bool Cache = true)
    {
        public Func<Page>? PageFactory { get; set; }
        public Func<Page, IEnumerable<PageCommand>>? PageCommandsFactory { get; set; }

        public override string ToString() => $"AppNavigationItem: {TargetPageType}";
    }
}
