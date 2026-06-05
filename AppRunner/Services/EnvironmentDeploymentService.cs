using System;
using System.IO;
using AppRunner.Models;

namespace AppRunner.Services
{
    public class EnvironmentDeploymentService
    {
        private readonly MachineEnvironmentService _machineEnvironmentService;
        private readonly ElevationService _elevationService;

        public EnvironmentDeploymentService(
            MachineEnvironmentService machineEnvironmentService,
            ElevationService elevationService)
        {
            _machineEnvironmentService = machineEnvironmentService;
            _elevationService = elevationService;
        }

        public void DeployEnvironment(RunEnvironment env, bool allowSelfElevation = true)
        {
            if (allowSelfElevation &&
                !_elevationService.IsRunningAsAdministrator())
            {
                _elevationService.StartElevatedSelf(
                    AppRunnerCommandLine.CreateDeployEnvironmentArguments(env.Guid));
                return;
            }

            if (Directory.Exists(env.WorkingDirectory))
            {
                Environment.CurrentDirectory = env.WorkingDirectory;
            }

            if (env.EnvironmentVariables is not null)
            {
                _machineEnvironmentService.SetVariables(env.EnvironmentVariables);
            }

            if (env.FileMaps is not null)
            {
                foreach (var fileMap in env.FileMaps)
                {
                    if (fileMap is null)
                    {
                        continue;
                    }

                    if (File.Exists(fileMap.Value) &&
                        !string.IsNullOrWhiteSpace(fileMap.Key))
                    {
                        var directory = Path.GetDirectoryName(fileMap.Key);
                        try
                        {
                            if (!Directory.Exists(directory))
                            {
                                Directory.CreateDirectory(directory!);
                            }

                            File.Copy(fileMap.Value, fileMap.Key, true);
                        }
                        catch
                        {

                        }
                    }
                }
            }
        }
    }
}
