using System;
using System.IO;

namespace AppRunner.Services
{
    public static class AppRunnerCommandLine
    {
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

        public static string GetExecutablePath()
        {
            return Environment.ProcessPath ?? Path.Combine(AppContext.BaseDirectory, "AppRunner.exe");
        }

        public static string Quote(string value)
        {
            return $"\"{value.Replace("\\", "\\\\").Replace("\"", "\\\"")}\"";
        }
    }
}
