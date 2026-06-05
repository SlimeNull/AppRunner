using System;
using System.Diagnostics;
using System.Security.Principal;

namespace AppRunner.Services
{
    public class ElevationService
    {
        public bool IsRunningAsAdministrator()
        {
            using var identity = WindowsIdentity.GetCurrent();
            var principal = new WindowsPrincipal(identity);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }

        public void StartElevatedSelf(string arguments)
        {
            Process.Start(new ProcessStartInfo()
            {
                FileName = AppRunnerCommandLine.GetAppExecutionAliasPath(),
                Arguments = arguments,
                UseShellExecute = true,
                Verb = "runas",
            });
        }
    }
}
