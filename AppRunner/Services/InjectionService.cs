using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using AppRunner.Injection;

namespace AppRunner.Services
{
    public class InjectionService
    {
        private readonly Assembly _currentAssembly;
        private readonly string _hookerDirectory = Path.Combine(AppContext.BaseDirectory, "data", "Hookers");

        public InjectionService()
        {
            _currentAssembly = Assembly.GetExecutingAssembly();
        }

        private async Task<string?> EnsureLocalFileHooker(string architecture)
        {
            var fileHookerFileName = $"AppRunner.FileHooker.{architecture}.dll";
            var fileHookerResourceName = $"AppRunner.Hookers.{fileHookerFileName}";

            using var fileHookerBinaryStream = _currentAssembly.GetManifestResourceStream(fileHookerResourceName);
            if (fileHookerBinaryStream is null)
            {
                return null;
            }

            Directory.CreateDirectory(_hookerDirectory);
            var resourceHash = await SHA256.HashDataAsync(fileHookerBinaryStream);
            var hashSuffix = Convert.ToHexString(resourceHash)[..16];
            var fileHookerPath = Path.Combine(_hookerDirectory, $"AppRunner.FileHooker.{architecture}.{hashSuffix}.dll");
            if (File.Exists(fileHookerPath))
            {
                await using var existingFile = File.OpenRead(fileHookerPath);
                var existingHash = await SHA256.HashDataAsync(existingFile);
                if (CryptographicOperations.FixedTimeEquals(resourceHash, existingHash))
                {
                    return fileHookerPath;
                }
            }

            fileHookerBinaryStream.Position = 0;
            var temporaryPath = Path.Combine(_hookerDirectory, $"{Guid.NewGuid():N}.tmp");
            try
            {
                await using (var temporaryFile = File.Create(temporaryPath))
                {
                    await fileHookerBinaryStream.CopyToAsync(temporaryFile);
                }
                File.Move(temporaryPath, fileHookerPath, true);
            }
            finally
            {
                if (File.Exists(temporaryPath)) File.Delete(temporaryPath);
            }

            return fileHookerPath;
        }

        public async Task<bool> InjectFileHookerAndWaitAsync(Process process)
        {
            var processInfo = new ProcessInfo(process);

            if (await EnsureLocalFileHooker(processInfo.Architecture) is not string fileHookerFullPath)
            {
                return false;
            }

            if (!File.Exists(fileHookerFullPath))
            {
                return false;
            }

            var procAddress = default(IntPtr);
            await Task.Run(() =>
            {
                procAddress = Injector.LoadLibraryInForeignProcess(processInfo, fileHookerFullPath);
            });

            return procAddress != IntPtr.Zero;
        }

    }
}
