using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppRunner.Helpers
{
    public class EnvironmentVariableModifier : IDisposable
    {
        private readonly Dictionary<string, string?> _environmentVariableOriginValues = new();

        private bool disposedValue;

        public void Set(string name, string? value)
        {
            if (!_environmentVariableOriginValues.ContainsKey(name))
            {
                _environmentVariableOriginValues[name] = Environment.GetEnvironmentVariable(name);
            }

            Environment.SetEnvironmentVariable(name, value);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    foreach (var nameValuePair in _environmentVariableOriginValues)
                    {
                        Environment.SetEnvironmentVariable(nameValuePair.Key, nameValuePair.Value);
                    }
                }

                disposedValue = true;
            }
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
