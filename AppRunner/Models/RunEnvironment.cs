using CommunityToolkit.Mvvm.ComponentModel;

namespace AppRunner.Models
{
    public class RunEnvironment : ObservableObject
    {
        public string WorkingDirectory { get; set; } = string.Empty;
        public Dictionary<string, string> EnvironmentVariables { get; } = new();
    }
}
