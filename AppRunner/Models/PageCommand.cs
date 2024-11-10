using System.Windows.Input;

namespace AppRunner.Models
{
    public record class PageCommand(string Icon, string Description, ICommand TargetCommand);
}
