using System;
using System.IO;

namespace AppRunner.Services
{
    public static class AppRunnerCommandLine
    {
        public const string AppExecutionAlias = "apprunner.exe";

        public static string CreateDeployEnvironmentArguments(Guid environmentGuid)
        {
            return $"deploy {Quote(environmentGuid.ToString())}";
        }

        public static string CreateRunApplicationArguments(
            Guid applicationGuid,
            Guid? environmentGuid = null,
            bool runAsAdministrator = false)
        {
            var arguments = $"run {Quote(applicationGuid.ToString())}";

            if (environmentGuid is not null)
            {
                arguments += $" --environment {Quote(environmentGuid.Value.ToString())}";
            }

            if (runAsAdministrator)
            {
                arguments += " --administrator";
            }

            return arguments;
        }

        public static string GetAppExecutionAliasPath()
        {
            var aliasPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "Microsoft",
                "WindowsApps",
                AppExecutionAlias);

            return File.Exists(aliasPath)
                ? aliasPath
                : AppExecutionAlias;
        }

        public static string Quote(string value)
        {
            return $"\"{value.Replace("\\", "\\\\").Replace("\"", "\\\"")}\"";
        }
    }
}
