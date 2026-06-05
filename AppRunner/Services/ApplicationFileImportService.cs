using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using AppRunner.Models;

namespace AppRunner.Services
{
    public class ApplicationFileImportService
    {
        private readonly ShortcutService _shortcutService;

        public ApplicationFileImportService(ShortcutService shortcutService)
        {
            _shortcutService = shortcutService;
        }

        public RunApp CreateApplicationFromFile(string path)
        {
            var fullPath = Path.GetFullPath(path);
            if (!File.Exists(fullPath))
            {
                throw new FileNotFoundException("The dropped file does not exist.", fullPath);
            }

            return Path.GetExtension(fullPath).ToLowerInvariant() switch
            {
                ".lnk" => CreateApplicationFromShortcut(fullPath),
                ".exe" => CreateApplicationFromExecutable(fullPath),
                _ => CreateApplicationFromAssociatedFile(fullPath),
            };
        }

        private RunApp CreateApplicationFromShortcut(string shortcutPath)
        {
            var shortcut = _shortcutService.ReadShortcut(shortcutPath);
            if (string.IsNullOrWhiteSpace(shortcut.TargetPath))
            {
                throw new InvalidOperationException("The dropped shortcut does not point to a file.");
            }

            return new RunApp()
            {
                Name = GetFileDisplayName(shortcutPath),
                FileName = shortcut.TargetPath,
                Description = shortcut.Description,
                CommandLineArguments = shortcut.Arguments,
                RunAsAdministrator = shortcut.RunAsAdministrator,
            };
        }

        private static RunApp CreateApplicationFromExecutable(string executablePath)
        {
            return new RunApp()
            {
                Name = GetExecutableDisplayName(executablePath),
                FileName = executablePath,
            };
        }

        private static RunApp CreateApplicationFromAssociatedFile(string filePath)
        {
            var executablePath = GetAssociatedExecutable(filePath);
            if (string.IsNullOrWhiteSpace(executablePath))
            {
                throw new InvalidOperationException("Can not find the default application for the dropped file.");
            }

            return new RunApp()
            {
                Name = GetFileDisplayName(filePath),
                FileName = executablePath,
                CommandLineArguments = QuoteCommandLineArgument(filePath),
            };
        }

        private static string GetExecutableDisplayName(string executablePath)
        {
            try
            {
                var versionInfo = FileVersionInfo.GetVersionInfo(executablePath);
                if (!string.IsNullOrWhiteSpace(versionInfo.FileDescription))
                {
                    return versionInfo.FileDescription;
                }

                if (!string.IsNullOrWhiteSpace(versionInfo.ProductName))
                {
                    return versionInfo.ProductName;
                }
            }
            catch
            {
            }

            return GetFileDisplayName(executablePath);
        }

        private static string GetFileDisplayName(string path)
        {
            return Path.GetFileNameWithoutExtension(path);
        }

        private static string QuoteCommandLineArgument(string argument)
        {
            var builder = new StringBuilder();
            builder.Append('"');

            var backslashCount = 0;
            foreach (var character in argument)
            {
                if (character == '\\')
                {
                    backslashCount++;
                    continue;
                }

                if (character == '"')
                {
                    builder.Append('\\', backslashCount * 2 + 1);
                    builder.Append(character);
                    backslashCount = 0;
                    continue;
                }

                builder.Append('\\', backslashCount);
                backslashCount = 0;
                builder.Append(character);
            }

            builder.Append('\\', backslashCount * 2);
            builder.Append('"');

            return builder.ToString();
        }

        private static string? GetAssociatedExecutable(string filePath)
        {
            var extension = Path.GetExtension(filePath);
            if (string.IsNullOrWhiteSpace(extension))
            {
                return null;
            }

            var length = 0u;
            var result = AssocQueryString(
                AssocQueryStringFlags.None,
                AssocQueryStringType.Executable,
                extension,
                "open",
                null,
                ref length);

            if (result != SFalse || length == 0)
            {
                return null;
            }

            var builder = new StringBuilder((int)length);
            result = AssocQueryString(
                AssocQueryStringFlags.None,
                AssocQueryStringType.Executable,
                extension,
                "open",
                builder,
                ref length);

            return result == SOk
                ? builder.ToString()
                : null;
        }

        private const int SOk = 0;
        private const int SFalse = 1;

        [DllImport("Shlwapi.dll", CharSet = CharSet.Unicode)]
        private static extern int AssocQueryString(
            AssocQueryStringFlags flags,
            AssocQueryStringType str,
            string pszAssoc,
            string? pszExtra,
            StringBuilder? pszOut,
            ref uint pcchOut);

        private enum AssocQueryStringFlags
        {
            None = 0,
        }

        private enum AssocQueryStringType
        {
            Executable = 2,
        }
    }
}
