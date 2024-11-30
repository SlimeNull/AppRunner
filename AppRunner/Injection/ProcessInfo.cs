using System.Diagnostics;
using AppRunner.Native;

namespace AppRunner.Injection
{
    public class ProcessInfo
    {
        public ProcessInfo(Process process)
        {
            this.Process = process ?? throw new ArgumentNullException(nameof(process));
            this.Id = process.Id;
            this.Handle = WindowsApi.OpenProcess(WindowsApi.ProcessAccessFlags.All, false, process.Id);

            this.Architecture = WindowsApi.GetArchitectureWithoutException(this.Process);

            this.SupportedFrameworkName = GetSupportedTargetFramework(process);
        }

        public Process Process { get; }

        public int Id { get; }

        public WindowsApi.ProcessHandle Handle { get; }

        public string Architecture { get; }

        public string SupportedFrameworkName { get; }

        public static ProcessInfo? FromWindowHandle(IntPtr handle)
        {
            var processFromWindowHandle = GetProcessFromWindowHandle(handle);

            if (processFromWindowHandle is null)
            {
                return null;
            }

            return new ProcessInfo(processFromWindowHandle);
        }

        private static Process? GetProcessFromWindowHandle(IntPtr windowHandle)
        {
            _ = WindowsApi.GetWindowThreadProcessId(windowHandle, out var processId);

            if (processId == 0)
            {
                return null;
            }

            try
            {
                var process = Process.GetProcessById(processId);
                return process;
            }
            catch (Exception e)
            {
                // pass
            }

            return null;
        }

        private static string GetSupportedTargetFramework(Process process)
        {
            var modules = WindowsApi.GetModules(process);

            FileVersionInfo? systemRuntimeVersion = null;
            FileVersionInfo? wpfGFXVersion = null;

            foreach (var module in modules)
            {
#if DEBUG
                var fileVersionInfo = FileVersionInfo.GetVersionInfo(module.szExePath);
#endif

                if (module.szModule.StartsWith("wpfgfx_", StringComparison.OrdinalIgnoreCase))
                {
                    wpfGFXVersion = FileVersionInfo.GetVersionInfo(module.szExePath);
                }
                else if (module.szModule.StartsWith("System.Runtime.dll", StringComparison.OrdinalIgnoreCase))
                {
                    systemRuntimeVersion = FileVersionInfo.GetVersionInfo(module.szExePath);
                }
            }

            var relevantVersionInfo = systemRuntimeVersion
            ?? wpfGFXVersion;

            if (relevantVersionInfo is null)
            {
                return "net462";
            }

            var productVersion = TryParseVersion(relevantVersionInfo.ProductVersion ?? string.Empty);
            return productVersion.Major switch
            {
                >= 6 => "net6.0-windows",
                4 => "net462",
                _ => throw new NotSupportedException($".NET version {relevantVersionInfo.ProductVersion} is not supported.")
            };
        }

        private static Version TryParseVersion(string version)
        {
            var versionToParse = version;

            var previewVersionMarkerIndex = versionToParse.IndexOfAny(new[] { '-', '+' });

            if (previewVersionMarkerIndex > -1)
            {
                versionToParse = version.Substring(0, previewVersionMarkerIndex);
            }

            if (Version.TryParse(versionToParse, out var parsedVersion))
            {
                return parsedVersion;
            }

            return new Version();
        }
    }
}
