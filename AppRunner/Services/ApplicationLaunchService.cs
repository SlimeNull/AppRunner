using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AppRunner.Helpers;
using AppRunner.Models;
using AppRunner.Resources;

namespace AppRunner.Services
{
    public class ApplicationLaunchService
    {
        private const string FileMapsEnvironmentVariableName = "SlimeNull.AppRunner.FileMaps";

        private readonly ConfigurationService _configurationService;
        private readonly InjectionService _injectionService;
        private readonly ElevationService _elevationService;
        private readonly ApplicationManifestService _applicationManifestService;

        public ApplicationLaunchService(
            ConfigurationService configurationService,
            InjectionService injectionService,
            ElevationService elevationService,
            ApplicationManifestService applicationManifestService)
        {
            _configurationService = configurationService;
            _injectionService = injectionService;
            _elevationService = elevationService;
            _applicationManifestService = applicationManifestService;
        }

        private void ApplyEnvironmentBeforeApplicationStart(
            EnvironmentVariableModifier environmentVariableModifier,
            ProcessStartInfo processStartInfo,
            RunEnvironment? env)
        {
            if (env is null)
            {
                processStartInfo.WorkingDirectory = _configurationService.Configuration.DefaultWorkingDirectory switch
                {
                    DefaultWorkingDirectory.WorkingDirectoryOfCurrentApp => Environment.CurrentDirectory,
                    DefaultWorkingDirectory.UserProfile => Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                    _ => string.Empty
                };

                return;
            }

            var workingDirectory = Environment.ExpandEnvironmentVariables(env.WorkingDirectory);

            if (!string.IsNullOrWhiteSpace(workingDirectory) && Directory.Exists(workingDirectory))
            {
                processStartInfo.WorkingDirectory = workingDirectory;
            }
            else
            {
                processStartInfo.WorkingDirectory = _configurationService.Configuration.DefaultWorkingDirectory switch
                {
                    DefaultWorkingDirectory.WorkingDirectoryOfCurrentApp => Environment.CurrentDirectory,
                    DefaultWorkingDirectory.UserProfile => Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                    _ => string.Empty
                };
            }

            if (env.EnvironmentVariables is not null)
            {
                foreach (var var in env.EnvironmentVariables)
                {
                    if (string.IsNullOrWhiteSpace(var.Key))
                    {
                        continue;
                    }

                    environmentVariableModifier.Set(var.Key, var.Value);
                }
            }

            if (env.FileMaps is not null)
            {
                var fileMapStrings = env.FileMaps
                    .Where(map => !string.IsNullOrWhiteSpace(map.Key) && !string.IsNullOrWhiteSpace(map.Value))
                    .Select(map => $"{Environment.ExpandEnvironmentVariables(map.Key!).Trim('"')}|{Environment.ExpandEnvironmentVariables(map.Value!).Trim('"')}");

                var environmentVariableValue = string.Join('|', fileMapStrings);
                environmentVariableModifier.Set(FileMapsEnvironmentVariableName, environmentVariableValue);
            }
            else
            {
                environmentVariableModifier.Set(FileMapsEnvironmentVariableName, null);
            }
        }

        private async Task ApplyEnvironmentAfterApplicationStarted(
            Process process,
            RunEnvironment? env)
        {
            if (env?.FileMaps is null)
            {
                return;
            }

            try
            {
                await _injectionService.InjectFileHookerAndWaitAsync(process);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"{Strings.Message_AnErrorOccurredWhilePerformingFileMappingOnTheTargetApplication}. {ex.Message}", ex);
            }
        }

        public async Task RunApplication(
            RunApp app,
            RunEnvironment? environmentOverride = null,
            bool? runAsAdministratorOverride = null,
            bool allowSelfElevation = true)
        {
            if (string.IsNullOrWhiteSpace(app.FileName))
            {
                throw new InvalidOperationException(Strings.Message_AppFileNameCanNotBeEmpty);
            }

            var trimmedAppFileName = app.FileName.Trim('"');
            var shouldRunAsAdministrator = runAsAdministratorOverride
                ?? (app.RunAsAdministrator ||
                    _applicationManifestService.RequiresAdministrator(trimmedAppFileName));

            var environmentGuid = environmentOverride?.Guid;
            var isRunningAsAdministrator = _elevationService.IsRunningAsAdministrator();

            if (shouldRunAsAdministrator &&
                allowSelfElevation &&
                !isRunningAsAdministrator)
            {
                _elevationService.StartElevatedSelf(
                    AppRunnerCommandLine.CreateRunApplicationArguments(
                        app.Guid,
                        environmentGuid,
                        runAsAdministrator: true));
                return;
            }

            var startInfo = new ProcessStartInfo()
            {
                FileName = Environment.ExpandEnvironmentVariables(trimmedAppFileName),
                Arguments = Environment.ExpandEnvironmentVariables(app.CommandLineArguments),
                CreateNoWindow = app.CreateNoWindow,
                UseShellExecute = app.UseShellExecute,
            };

            if (shouldRunAsAdministrator && !isRunningAsAdministrator)
            {
                startInfo.UseShellExecute = true;
                startInfo.Verb = "runas";
            }

            var appEnvironment = _configurationService.Configuration.Environments?
                .FirstOrDefault(env => env.Guid == app.EnvironmentGuid);

            using var environmentVariableModifier = new EnvironmentVariableModifier();
            var environment = environmentOverride ?? appEnvironment;
            ApplyEnvironmentBeforeApplicationStart(environmentVariableModifier, startInfo, environment);

            var process = Process.Start(startInfo);
            if (process is null)
            {
                return;
            }

            await ApplyEnvironmentAfterApplicationStarted(process, environment);

            await Task.Run(() =>
            {
                try
                {
                    process.WaitForInputIdle(TimeSpan.FromSeconds(1));
                }
                catch { }
            });
        }
    }
}
