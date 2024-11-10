using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;

namespace AppRunner.Models
{
    public record class AppNavigationItem(string Icon, string Title, Page TargetPage)
    {
        public IList<PageCommand> PageCommands { get; } = new List<PageCommand>();
    }
}
