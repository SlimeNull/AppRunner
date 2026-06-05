using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Xml.Linq;
using Microsoft.Win32;

namespace AppRunner.Services
{
    public class ApplicationManifestService
    {
        private const int ResourceTypeManifest = 24;
        private const int LoadLibraryAsDatafile = 0x00000002;
        private const string AppCompatLayersRegistryKeyPath = @"Software\Microsoft\Windows NT\CurrentVersion\AppCompatFlags\Layers";
        private const string RunAsAdministratorCompatibilityLayer = "RUNASADMIN";

        public bool RequiresAdministrator(string fileName)
        {
            var executablePath = ResolveExecutablePath(fileName);
            if (executablePath is null)
            {
                return false;
            }

            if (CompatibilityLayerRequiresAdministrator(executablePath))
            {
                return true;
            }

            var externalManifestPath = executablePath + ".manifest";
            if (File.Exists(externalManifestPath) &&
                ManifestRequiresAdministrator(ReadManifestFile(externalManifestPath)))
            {
                return true;
            }

            return ManifestRequiresAdministrator(ReadEmbeddedManifest(executablePath));
        }

        private static string? ResolveExecutablePath(string fileName)
        {
            var expandedFileName = Environment.ExpandEnvironmentVariables(fileName.Trim('"'));
            if (string.IsNullOrWhiteSpace(expandedFileName) ||
                !string.Equals(Path.GetExtension(expandedFileName), ".exe", StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            if (Path.IsPathRooted(expandedFileName))
            {
                return File.Exists(expandedFileName)
                    ? expandedFileName
                    : null;
            }

            var currentDirectoryPath = Path.Combine(Environment.CurrentDirectory, expandedFileName);
            if (File.Exists(currentDirectoryPath))
            {
                return currentDirectoryPath;
            }

            foreach (var directory in (Environment.GetEnvironmentVariable("PATH") ?? string.Empty)
                .Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries))
            {
                try
                {
                    var path = Path.Combine(directory.Trim('"'), expandedFileName);
                    if (File.Exists(path))
                    {
                        return path;
                    }
                }
                catch
                {
                }
            }

            return null;
        }

        private static bool CompatibilityLayerRequiresAdministrator(string executablePath)
        {
            var registryViews = Environment.Is64BitOperatingSystem
                ? new[] { RegistryView.Registry64, RegistryView.Registry32 }
                : new[] { RegistryView.Registry32 };

            foreach (var registryView in registryViews)
            {
                if (CompatibilityLayerRequiresAdministrator(executablePath, RegistryHive.CurrentUser, registryView) ||
                    CompatibilityLayerRequiresAdministrator(executablePath, RegistryHive.LocalMachine, registryView))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool CompatibilityLayerRequiresAdministrator(
            string executablePath,
            RegistryHive registryHive,
            RegistryView registryView)
        {
            try
            {
                using var baseKey = RegistryKey.OpenBaseKey(registryHive, registryView);
                using var layersKey = baseKey.OpenSubKey(AppCompatLayersRegistryKeyPath);
                if (layersKey is null)
                {
                    return false;
                }

                if (GetCompatibilityLayerValue(layersKey, executablePath) is not string compatibilityLayers)
                {
                    return false;
                }

                return compatibilityLayers
                    .Split(' ', StringSplitOptions.RemoveEmptyEntries)
                    .Any(layer => string.Equals(
                        layer.TrimStart('~'),
                        RunAsAdministratorCompatibilityLayer,
                        StringComparison.OrdinalIgnoreCase));
            }
            catch
            {
                return false;
            }
        }

        private static string? GetCompatibilityLayerValue(RegistryKey layersKey, string executablePath)
        {
            if (layersKey.GetValue(executablePath) is string value)
            {
                return value;
            }

            foreach (var valueName in layersKey.GetValueNames())
            {
                if (string.Equals(valueName, executablePath, StringComparison.OrdinalIgnoreCase) &&
                    layersKey.GetValue(valueName) is string matchingValue)
                {
                    return matchingValue;
                }
            }

            return null;
        }

        private static string? ReadManifestFile(string path)
        {
            try
            {
                return File.ReadAllText(path);
            }
            catch
            {
                return null;
            }
        }

        private static string? ReadEmbeddedManifest(string executablePath)
        {
            var module = LoadLibraryEx(executablePath, IntPtr.Zero, LoadLibraryAsDatafile);
            if (module == IntPtr.Zero)
            {
                return null;
            }

            try
            {
                for (var resourceId = 1; resourceId <= 3; resourceId++)
                {
                    var resource = FindResource(module, new IntPtr(resourceId), new IntPtr(ResourceTypeManifest));
                    if (resource == IntPtr.Zero)
                    {
                        continue;
                    }

                    var size = SizeofResource(module, resource);
                    if (size == 0)
                    {
                        continue;
                    }

                    var resourceHandle = LoadResource(module, resource);
                    if (resourceHandle == IntPtr.Zero)
                    {
                        continue;
                    }

                    var resourcePointer = LockResource(resourceHandle);
                    if (resourcePointer == IntPtr.Zero)
                    {
                        continue;
                    }

                    var bytes = new byte[size];
                    Marshal.Copy(resourcePointer, bytes, 0, (int)size);
                    return DecodeManifest(bytes);
                }
            }
            finally
            {
                FreeLibrary(module);
            }

            return null;
        }

        private static string DecodeManifest(byte[] bytes)
        {
            if (bytes.Length >= 2 && bytes[0] == 0xFF && bytes[1] == 0xFE)
            {
                return Encoding.Unicode.GetString(bytes);
            }

            if (bytes.Length >= 2 && bytes[0] == 0xFE && bytes[1] == 0xFF)
            {
                return Encoding.BigEndianUnicode.GetString(bytes);
            }

            return Encoding.UTF8.GetString(bytes);
        }

        private static bool ManifestRequiresAdministrator(string? manifest)
        {
            if (string.IsNullOrWhiteSpace(manifest))
            {
                return false;
            }

            try
            {
                var document = XDocument.Parse(manifest);
                foreach (var element in document.Descendants())
                {
                    if (!string.Equals(element.Name.LocalName, "requestedExecutionLevel", StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    var level = element.Attribute("level")?.Value;
                    if (string.Equals(level, "requireAdministrator", StringComparison.OrdinalIgnoreCase) ||
                        string.Equals(level, "highestAvailable", StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }
                }
            }
            catch
            {
            }

            return false;
        }

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern IntPtr LoadLibraryEx(string lpFileName, IntPtr hFile, int dwFlags);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool FreeLibrary(IntPtr hModule);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr FindResource(IntPtr hModule, IntPtr lpName, IntPtr lpType);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern uint SizeofResource(IntPtr hModule, IntPtr hResInfo);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr LoadResource(IntPtr hModule, IntPtr hResInfo);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr LockResource(IntPtr hResData);
    }
}
