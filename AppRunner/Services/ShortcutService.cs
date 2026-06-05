using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace AppRunner.Services
{
    public class ShortcutService
    {
        public void CreateShortcut(
            string shortcutPath,
            string arguments,
            string description)
        {
            var shellLink = (IShellLinkW)new ShellLink();
            shellLink.SetPath(AppRunnerCommandLine.GetAppExecutionAliasPath());
            shellLink.SetArguments(arguments);
            shellLink.SetDescription(description);
            shellLink.SetIconLocation(AppRunnerCommandLine.GetAppExecutionAliasPath(), 0);

            if (Path.GetDirectoryName(shortcutPath) is string workingDirectory)
            {
                shellLink.SetWorkingDirectory(workingDirectory);
            }

            ((IPersistFile)shellLink).Save(shortcutPath, true);
        }

        public static string GetSafeShortcutFileName(string name)
        {
            var builder = new StringBuilder();
            foreach (var character in name)
            {
                builder.Append(Array.IndexOf(Path.GetInvalidFileNameChars(), character) >= 0
                    ? '_'
                    : character);
            }

            var fileName = builder.ToString().Trim();
            return string.IsNullOrWhiteSpace(fileName)
                ? "AppRunner"
                : fileName;
        }

        [ComImport]
        [Guid("00021401-0000-0000-C000-000000000046")]
        private class ShellLink
        {
        }

        [ComImport]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        [Guid("000214F9-0000-0000-C000-000000000046")]
        private interface IShellLinkW
        {
            void GetPath(
                [Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszFile,
                int cchMaxPath,
                IntPtr pfd,
                uint fFlags);

            void GetIDList(out IntPtr ppidl);
            void SetIDList(IntPtr pidl);
            void GetDescription([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszName, int cchMaxName);
            void SetDescription([MarshalAs(UnmanagedType.LPWStr)] string pszName);
            void GetWorkingDirectory([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszDir, int cchMaxPath);
            void SetWorkingDirectory([MarshalAs(UnmanagedType.LPWStr)] string pszDir);
            void GetArguments([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszArgs, int cchMaxPath);
            void SetArguments([MarshalAs(UnmanagedType.LPWStr)] string pszArgs);
            void GetHotkey(out short pwHotkey);
            void SetHotkey(short wHotkey);
            void GetShowCmd(out int piShowCmd);
            void SetShowCmd(int iShowCmd);
            void GetIconLocation([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszIconPath, int cchIconPath, out int piIcon);
            void SetIconLocation([MarshalAs(UnmanagedType.LPWStr)] string pszIconPath, int iIcon);
            void SetRelativePath([MarshalAs(UnmanagedType.LPWStr)] string pszPathRel, uint dwReserved);
            void Resolve(IntPtr hwnd, uint fFlags);
            void SetPath([MarshalAs(UnmanagedType.LPWStr)] string pszFile);
        }

        [ComImport]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        [Guid("0000010b-0000-0000-C000-000000000046")]
        private interface IPersistFile
        {
            void GetClassID(out Guid pClassID);
            void IsDirty();
            void Load([MarshalAs(UnmanagedType.LPWStr)] string pszFileName, uint dwMode);
            void Save([MarshalAs(UnmanagedType.LPWStr)] string pszFileName, bool fRemember);
            void SaveCompleted([MarshalAs(UnmanagedType.LPWStr)] string pszFileName);
            void GetCurFile([MarshalAs(UnmanagedType.LPWStr)] out string ppszFileName);
        }
    }
}
