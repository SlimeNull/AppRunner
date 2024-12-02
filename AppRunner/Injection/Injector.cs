using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using AppRunner.Native;

namespace AppRunner.Injection
{
    public class Injector
    {
        /// <summary>
        /// Loads a library into a foreign process and returns the module handle of the loaded library.
        /// </summary>
        public static IntPtr LoadLibraryInForeignProcess(ProcessInfo processWrapper, string pathToDll)
        {
            if (File.Exists(pathToDll) == false)
            {
                throw new FileNotFoundException("Could not find file for loading in foreign process.", pathToDll);
            }

            var stringForRemoteProcess = pathToDll;

            var bufLen = (stringForRemoteProcess.Length + 1) * Marshal.SizeOf(typeof(char));
            var remoteAddress = WindowsApi.VirtualAllocEx(processWrapper.Handle, IntPtr.Zero, (uint)bufLen, WindowsApi.AllocationType.Commit, WindowsApi.MemoryProtection.ReadWrite);

            if (remoteAddress == IntPtr.Zero)
            {
                throw new Win32Exception();
            }

            var address = Marshal.StringToHGlobalUni(stringForRemoteProcess);
            var size = (uint)(sizeof(char) * stringForRemoteProcess.Length);

            try
            {
                var writeProcessMemoryResult = WindowsApi.WriteProcessMemory(processWrapper.Handle, remoteAddress, address, size, out var bytesWritten);

                if (writeProcessMemoryResult == false
                    || bytesWritten == 0)
                {
                    throw Marshal.GetExceptionForHR(Marshal.GetLastWin32Error()) ?? new Exception("Unknown error while trying to write to foreign process memory.");
                }

                var hLibrary = WindowsApi.GetModuleHandle("kernel32");

                // Load dll into the remote process
                // (via CreateRemoteThread & LoadLibrary)
                var procAddress = WindowsApi.GetProcAddress(hLibrary, "LoadLibraryW");

                if (procAddress == IntPtr.Zero)
                {
                    throw new Win32Exception();
                }

                var remoteThread = WindowsApi.CreateRemoteThread(processWrapper.Handle,
                    IntPtr.Zero,
                    0,
                    procAddress,
                    remoteAddress,
                    0,
                    out _);

                IntPtr moduleHandleInForeignProcess;
                try
                {
                    if (remoteThread == IntPtr.Zero)
                    {
                        throw new Win32Exception();
                    }
                    else
                    {
                        WindowsApi.WaitForSingleObject(remoteThread);

                        // Get handle of the loaded module
                        if (WindowsApi.GetExitCodeThread(remoteThread, out moduleHandleInForeignProcess) == false)
                        {
                            throw new Win32Exception();
                        }
                    }
                }
                finally
                {
                    WindowsApi.CloseHandle(remoteThread);
                }

                try
                {
                    WindowsApi.VirtualFreeEx(processWrapper.Handle, remoteAddress, bufLen, WindowsApi.AllocationType.Release);
                }
                catch (Exception e)
                {
                    // pass
                }

                if (moduleHandleInForeignProcess == IntPtr.Zero)
                {
                    throw new Exception($"Could not load \"{pathToDll}\" in process \"{processWrapper.Id}\".");
                }

                var remoteHandle = WindowsApi.GetRemoteModuleHandle(processWrapper.Process, Path.GetFileName(pathToDll));

                return remoteHandle;
            }
            finally
            {
                Marshal.FreeHGlobal(address);
            }
        }
    }
}
