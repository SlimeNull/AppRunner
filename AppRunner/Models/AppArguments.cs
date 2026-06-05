using System;
using CommandLine;

namespace AppRunner.Models
{
    abstract class AppArgumentsBase
    {
        [Option('n', "language")]
        public string? Language { get; set; }
    }

    [Verb("app", isDefault: true)]
    class AppArguments : AppArgumentsBase
    {
    }

    [Verb("deploy")]
    class DeployEnvironmentArguments : AppArgumentsBase
    {
        [Value(0, Required = true)]
        public Guid EnvironmentGuid { get; set; }
    }

    [Verb("run")]
    class RunApplicationArguments : AppArgumentsBase
    {
        [Value(0, Required = true)]
        public Guid ApplicationGuid { get; set; }

        [Option('e', "environment")]
        public Guid? EnvironmentGuid { get; set; }

        [Option('a', "administrator")]
        public bool RunAsAdministrator { get; set; }
    }
}
