using AppRunner.Abstraction;
using AppRunner.Data;
using AppRunner.Resources;
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

        public List<ReferenceKeyValuePair<string, string>>? FileMaps { get; set; }

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

        public void Populate(RunEnvironment runEnv)
        {
            runEnv.Name = Name;
            runEnv.Description = Description;
            runEnv.WorkingDirectory = WorkingDirectory;
            runEnv.Group = Group;

            if (EnvironmentVariables is null)
            {
                runEnv.EnvironmentVariables = null;
            }
            else
            {
                runEnv.EnvironmentVariables = EnvironmentVariables
                    .Select(p => p with { })
                    .ToList();
            }

            if (FileMaps is null)
            {
                runEnv.FileMaps = null;
            }
            else
            {
                runEnv.FileMaps = FileMaps
                    .Select(p => p with { })
                    .ToList();
            }
        }

        public static RunEnvironment Empty { get; } = new RunEnvironment()
        {
            Name = Strings.Common_None,
            Guid = default,
        };
    }
}
