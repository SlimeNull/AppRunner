using AppRunner.Abstraction;
using AppRunner.Data;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AppRunner.Models
{
    public partial class RunEnvironment : ObservableObject, IGroupable
    {
        public Guid Guid { get; set; } = Guid.NewGuid();

        [ObservableProperty]
        private string _name = string.Empty;

        [ObservableProperty]
        private string _description = string.Empty;

        [ObservableProperty]
        private string _workingDirectory = string.Empty;

        [ObservableProperty]
        private string _group = string.Empty;

        public List<ReferenceKeyValuePair<string, string>>? EnvironmentVariables { get; set; } 

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
            runApp.Group = Group;
            
            if (EnvironmentVariables is null)
            {
                runApp.EnvironmentVariables = null;
            }
            else
            {
                runApp.EnvironmentVariables = EnvironmentVariables
                    .Select(p => p with { })
                    .ToList();
            }
        }
    }
}
