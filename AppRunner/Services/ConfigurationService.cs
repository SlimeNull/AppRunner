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
        private AppConfiguration? _loadedConfiguration;

        public string ConfigurationPath => System.IO.Path.Combine(AppContext.BaseDirectory, "AppConfiguration.json");

        public AppConfiguration Configuration => _loadedConfiguration ??= LoadConfiguration();

        public void SaveConfiguration()
        {
            var configurationText = JsonSerializer.Serialize(Configuration);
            File.WriteAllText(ConfigurationPath, configurationText);
        }

        private AppConfiguration LoadConfiguration()
        {
            if (!File.Exists(ConfigurationPath))
            {
                return new AppConfiguration();
            }

            try
            {
                var configurationText = File.ReadAllText(ConfigurationPath);
                var configuration = JsonSerializer.Deserialize<AppConfiguration>(configurationText);

                if (configuration is null)
                {
                    return new AppConfiguration();
                }

                return configuration;
            }
            catch
            {
                return new AppConfiguration();
            }
        }
    }
}
