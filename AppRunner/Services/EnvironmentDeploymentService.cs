using System;
using System.IO;
using AppRunner.Models;

namespace AppRunner.Services
{
    public class EnvironmentDeploymentService
    {
        public void DeployEnvironment(RunEnvironment env)
        {
            if (Directory.Exists(env.WorkingDirectory))
            {
                Environment.CurrentDirectory = env.WorkingDirectory;
            }

            if (env.EnvironmentVariables is not null)
            {
                foreach (var var in env.EnvironmentVariables)
                {
                    if (string.IsNullOrWhiteSpace(var.Key))
                    {
                        continue;
                    }

                    Environment.SetEnvironmentVariable(var.Key, var.Value, EnvironmentVariableTarget.Machine);
                }
            }

            if (env.FileMaps is not null)
            {
                foreach (var fileMap in env.FileMaps)
                {
                    if (fileMap is null)
                    {
                        continue;
                    }

                    if (File.Exists(fileMap.Value) &&
                        !string.IsNullOrWhiteSpace(fileMap.Key))
                    {
                        var directory = Path.GetDirectoryName(fileMap.Key);
                        try
                        {
                            if (!Directory.Exists(directory))
                            {
                                Directory.CreateDirectory(directory!);
                            }

                            File.Copy(fileMap.Value, fileMap.Key, true);
                        }
                        catch
                        {

                        }
                    }
                }
            }
        }
    }
}
