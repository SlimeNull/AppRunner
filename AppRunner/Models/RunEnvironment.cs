using AppRunner.Data;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AppRunner.Models
{
    public partial class RunEnvironment : ObservableObject
    {
        public Guid Guid { get; } = Guid.NewGuid();

        [ObservableProperty]
        private string _name = string.Empty;

        [ObservableProperty]
        private string _description = string.Empty;

        [ObservableProperty]
        private string _workingDirectory = string.Empty;

        public List<ReferenceKeyValuePair<string, string>> EnvironmentVariables { get; } = new();

        private void HandleStartException(Exception ex)
        {
            // TODO: show error message
        }

        public RunEnvironment Clone()
        {
            var newApp = new RunEnvironment();
            Populate(newApp);

            return newApp;
        }

        public void Populate(RunEnvironment runApp)
        {
            runApp.Name = Name;
            runApp.Description = Description;
            runApp.WorkingDirectory = WorkingDirectory;
            runApp.EnvironmentVariables.Clear();
            runApp.EnvironmentVariables.AddRange(EnvironmentVariables.Select(v => v with { }));
        }

        [RelayCommand]
        public void Apply()
        {

        }

        [RelayCommand]
        public void ApplyVariablesToSystem()
        {

        }
    }
}
