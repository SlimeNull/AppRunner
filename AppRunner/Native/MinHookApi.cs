using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace AppRunner.Helpers
{
    public static class MinHookApi
    {

        // Initialize the MinHook library. You must call this function EXACTLY ONCE at the beginning of your program.
        [DllImport("MinHook.dll", CallingConvention = CallingConvention.Winapi, EntryPoint = "MH_Initialize")]
        public static extern Status Initialize();

        // Uninitialize the MinHook library. You must call this function EXACTLY ONCE at the end of your program.
        [DllImport("MinHook.dll", CallingConvention = CallingConvention.Winapi, EntryPoint = "MH_Uninitialize")]
        public static extern Status Uninitialize();

        // Creates a Hook for the specified target function, in disabled state.
        [DllImport("MinHook.dll", CallingConvention = CallingConvention.Winapi, EntryPoint = "MH_CreateHook")]
        public static extern Status CreateHook(IntPtr pTarget, IntPtr pDetour, out IntPtr ppOriginal);

        // Creates a Hook for the specified API function, in disabled state.
        [DllImport("MinHook.dll", CallingConvention = CallingConvention.Winapi, EntryPoint = "MH_CreateHookApi")]
        public static extern Status CreateHookApi(
            string pszModule, string pszProcName, IntPtr pDetour, out IntPtr ppOriginal);

        // Creates a Hook for the specified API function, in disabled state.
        [DllImport("MinHook.dll", CallingConvention = CallingConvention.Winapi, EntryPoint = "MH_CreateHookApiEx")]
        public static extern Status CreateHookApiEx(
            string pszModule, string pszProcName, IntPtr pDetour, out IntPtr ppOriginal, out IntPtr ppTarget);

        // Removes an already created hook.
        [DllImport("MinHook.dll", CallingConvention = CallingConvention.Winapi, EntryPoint = "MH_RemoveHook")]
        public static extern Status RemoveHook(IntPtr pTarget);

        // Enables an already created hook.
        [DllImport("MinHook.dll", CallingConvention = CallingConvention.Winapi, EntryPoint = "MH_EnableHook")]
        public static extern Status EnableHook(IntPtr pTarget);

        // Disables an already created hook.
        [DllImport("MinHook.dll", CallingConvention = CallingConvention.Winapi, EntryPoint = "MH_DisableHook")]
        public static extern Status DisableHook(IntPtr pTarget);

        // Queues to enable an already created hook.
        [DllImport("MinHook.dll", CallingConvention = CallingConvention.Winapi, EntryPoint = "MH_QueueEnableHook")]
        public static extern Status QueueEnableHook(IntPtr pTarget);

        // Queues to disable an already created hook.
        [DllImport("MinHook.dll", CallingConvention = CallingConvention.Winapi, EntryPoint = "MH_QueueDisableHook")]
        public static extern Status QueueDisableHook(IntPtr pTarget);

        // Applies all queued changes in one go.
        [DllImport("MinHook.dll", CallingConvention = CallingConvention.Winapi, EntryPoint = "MH_ApplyQueued")]
        public static extern Status ApplyQueued();

        // Translates the MH_STATUS to its name as a string.
        [DllImport("MinHook.dll", CallingConvention = CallingConvention.Winapi, EntryPoint = "MH_StatusToString")]
        public static extern IntPtr StatusToString(int status);


        public enum Status
        {
            // Unknown error. Should not be returned.
            Unknown = -1,

            // Successful.
            OK = 0,

            // MinHook is already initialized.
            ErrorAlreadyInitialized,

            // MinHook is not initialized yet, or already uninitialized.
            ErrorNotInitialized,

            // The hook for the specified target function is already created.
            ErrorAlreadyCreated,

            // The hook for the specified target function is not created yet.
            ErrorNotCreated,

            // The hook for the specified target function is already enabled.
            ErrorEnabled,

            // The hook for the specified target function is not enabled yet, or already
            // disabled.
            ErrorDisabled,

            // The specified pointer is invalid. It points the address of non-allocated
            // and/or non-executable region.
            ErrorNotExecutable,

            // The specified target function cannot be hooked.
            ErrorUnsupportedFunction,

            // Failed to allocate memory.
            ErrorMemoryAlloc,

            // Failed to change the memory protection.
            ErrorMemoryProtect,

            // The specified module is not loaded.
            ErrorModuleNotFound,

            // The specified function is not found.
            ErrorFunctionNotFound
        }
    }
}
