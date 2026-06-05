using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using AppRunner.Data;
using Microsoft.Win32;

namespace AppRunner.Services
{
    public class MachineEnvironmentService
    {
        private const string EnvironmentRegistryKeyPath = @"SYSTEM\CurrentControlSet\Control\Session Manager\Environment";
        private const int HwndBroadcast = 0xFFFF;
        private const int WmSettingChange = 0x001A;
        private const int SmtoAbortIfHung = 0x0002;

        public void SetVariables(IEnumerable<ReferenceKeyValuePair<string, string>> variables)
        {
            var variablesToSet = variables
                .Where(variable => variable is not null && !string.IsNullOrWhiteSpace(variable.Key))
                .ToArray();

            if (variablesToSet.Length == 0)
            {
                return;
            }

            using var environmentKey = Registry.LocalMachine.OpenSubKey(EnvironmentRegistryKeyPath, writable: true)
                ?? throw new InvalidOperationException("Can not open machine environment registry key.");

            foreach (var variable in variablesToSet)
            {
                if (variable.Value is null)
                {
                    environmentKey.DeleteValue(variable.Key!, false);
                    continue;
                }

                environmentKey.SetValue(
                    variable.Key!,
                    variable.Value,
                    GetRegistryValueKind(variable.Value));
            }

            BroadcastEnvironmentChanged();
        }

        private static RegistryValueKind GetRegistryValueKind(string value)
        {
            return value.Contains('%')
                ? RegistryValueKind.ExpandString
                : RegistryValueKind.String;
        }

        private static void BroadcastEnvironmentChanged()
        {
            SendMessageTimeout(
                new IntPtr(HwndBroadcast),
                WmSettingChange,
                IntPtr.Zero,
                "Environment",
                SmtoAbortIfHung,
                1000,
                out _);
        }

        [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern IntPtr SendMessageTimeout(
            IntPtr hWnd,
            int msg,
            IntPtr wParam,
            string lParam,
            int flags,
            int timeout,
            out IntPtr result);
    }
}
