using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using AppRunner.Injection;
using Windows.Storage;

namespace AppRunner.Services
{
    public class InjectionService
    {
        private Assembly _currentAssembly;
        private StorageFolder _localStorageFolder;

        public InjectionService()
        {
            _currentAssembly = Assembly.GetExecutingAssembly();
            _localStorageFolder = ApplicationData.Current.LocalFolder;
        }

        private async Task<StorageFile?> EnsureLocalFileHooker(string architecture)
        {
            var fileHookerFileName = $"AppRunner.FileHooker.{architecture}.dll";
            var fileHookerResourceName = $"AppRunner.Hookers.{fileHookerFileName}";

            var existedItem = await _localStorageFolder.TryGetItemAsync(fileHookerFileName);
            if (existedItem is StorageFile existedFile)
            {
                return existedFile;
            }

            var fileHookerBinaryStream = _currentAssembly.GetManifestResourceStream(fileHookerResourceName);
            if (fileHookerBinaryStream is null)
            {
                return null;
            }

            var newFile = await _localStorageFolder.CreateFileAsync(fileHookerFileName);
            using var newFileStream = await newFile.OpenStreamForWriteAsync();
            await fileHookerBinaryStream.CopyToAsync(newFileStream);

            return newFile;
        }

        public async Task<bool> InjectFileHookerAndWaitAsync(Process process)
        {
            var processInfo = new ProcessInfo(process);

            if (await EnsureLocalFileHooker(processInfo.Architecture) is not StorageFile fileHooker)
            {
                return false;
            }

            string fileHookerFullPath = Path.Combine(fileHooker.Path);

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
