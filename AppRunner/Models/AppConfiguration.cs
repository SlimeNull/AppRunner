using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;

namespace AppRunner.Models
{
    public partial class AppConfiguration : ObservableObject
    {
        [ObservableProperty]
        private RunApp[]? _applications;

        [ObservableProperty]
        private RunEnvironment[]? _environments;

        [ObservableProperty]
        private DefaultWorkingDirectory _defaultWorkingDirectory;

        [ObservableProperty]
        private bool _applicationsShowGroupView;

        [ObservableProperty]
        private bool _environmentsShowGroupView;

        [ObservableProperty]
        private Dictionary<string, bool>? _applicationGroupExpandedValues;

        [ObservableProperty]
        private Dictionary<string, bool>? _environmentGroupExpandedValues;

        public bool UpdateApplicationGroupExpandedValue(string groupName, bool expanded)
        {
            ApplicationGroupExpandedValues = ApplicationGroupExpandedValues ?? new();

            if (!ApplicationGroupExpandedValues.TryGetValue(groupName, out var originValue))
            {
                ApplicationGroupExpandedValues[groupName] = expanded;
                return false;
            }

            ApplicationGroupExpandedValues[groupName] = expanded;
            return originValue != expanded;
        }

        public bool UpdateEnvironmentGroupExpandedValue(string groupName, bool expanded)
        {
            EnvironmentGroupExpandedValues = EnvironmentGroupExpandedValues ?? new();

            if (!EnvironmentGroupExpandedValues.TryGetValue(groupName, out var originValue))
            {
                EnvironmentGroupExpandedValues[groupName] = expanded;
                return false;
            }

            EnvironmentGroupExpandedValues[groupName] = expanded;
            return originValue != expanded;
        }

        public void TrimGroupExpandedValues()
        {
            if (ApplicationGroupExpandedValues is not null)
            {
                foreach (var key in ApplicationGroupExpandedValues.Keys.ToArray())
                {
                    if (Applications is null ||
                        Applications.Any(app => app.Group == key))
                    {
                        continue;
                    }

                    ApplicationGroupExpandedValues.Remove(key);
                }
            }

            if (EnvironmentGroupExpandedValues is not null)
            {
                foreach (var key in EnvironmentGroupExpandedValues.Keys.ToArray())
                {
                    if (Environments is null ||
                        Environments.Any(env => env.Group == key))
                    {
                        continue;
                    }

                    EnvironmentGroupExpandedValues.Remove(key);
                }
            }
        }
    }
}
