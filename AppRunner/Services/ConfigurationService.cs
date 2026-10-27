using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using AppRunner.Models;

namespace AppRunner.Services
{
    public class ConfigurationService
    {
        private const string FileName = "AppConfiguration.json";
        private AppConfiguration? _loadedConfiguration;
        private readonly string _configurationPath = Path.Combine(AppContext.BaseDirectory, "data", FileName);

        public string ConfigurationFileName => FileName;
        public AppConfiguration Configuration => _loadedConfiguration ?? throw new InvalidOperationException("Configuration not loaded");

        public async Task LoadConfiguration()
        {
            if (!File.Exists(_configurationPath))
            {
                _loadedConfiguration = new AppConfiguration();
                return;
            }

            try
            {
                var configurationText = await File.ReadAllTextAsync(_configurationPath);
                var configuration = JsonSerializer.Deserialize<AppConfiguration>(configurationText);

                _loadedConfiguration = configuration ?? new();
            }
            catch
            {
                _loadedConfiguration = new();
            }
        }

        public async Task SaveConfiguration()
        {
            var configurationText = JsonSerializer.Serialize(Configuration);
            Directory.CreateDirectory(Path.GetDirectoryName(_configurationPath)!);
            await File.WriteAllTextAsync(_configurationPath, configurationText);
        }

        public async Task ExportTo(string path)
        {
            var configurationText = JsonSerializer.Serialize(Configuration);
            using var fileStream = File.Create(path);
            using var fileStreamWriter = new StreamWriter(fileStream);

            await fileStreamWriter.WriteAsync(configurationText);
        }

        public async Task<bool> ImportFrom(string path)
        {
            try
            {
                using var fileStream = File.OpenRead(path);
                using var fileStreamReader = new StreamReader(fileStream);
                var configurationText = await fileStreamReader.ReadToEndAsync();
                var configuration = JsonSerializer.Deserialize<AppConfiguration>(configurationText);

                if (configuration is null)
                {
                    return false;
                }

                if (configuration.Environments is not null)
                {
                    if (Configuration.Environments is null)
                    {
                        Configuration.Environments = configuration.Environments;
                    }
                    else
                    {
                        Configuration.Environments = Configuration.Environments
                            .Concat(configuration.Environments.Where(env => !Configuration.Environments.Any(existEnv => existEnv.Guid == env.Guid)))
                            .ToArray();
                    }
                }

                if (configuration.Applications is not null)
                {
                    if (Configuration.Applications is null)
                    {
                        Configuration.Applications = configuration.Applications;
                    }
                    else
                    {
                        Configuration.Applications = Configuration.Applications
                            .Concat(configuration.Applications.Where(app => !Configuration.Applications.Any(existApp => existApp.Guid == app.Guid)))
                            .ToArray();
                    }
                }

                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
