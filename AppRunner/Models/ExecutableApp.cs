using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;

namespace AppRunner.Models
{
    public partial class ExecutableApp : RunApp
    {
        [ObservableProperty]
        private string? _filePath;

        [ObservableProperty]
        private string? _arguments;

        public ExecutableApp()
        {

        }

        public override async Task StartAsync()
        {
            ProcessStartInfo startInfo = new ProcessStartInfo()
            {
                FileName = FilePath,
                Arguments = Arguments,
            };
        }
    }
}
