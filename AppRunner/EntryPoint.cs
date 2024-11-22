using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Resources;
using System.Text;
using System.Threading.Tasks;
using AppRunner.Models;
using AppRunner.Resources;
using CommandLine;

namespace AppRunner
{
    class EntryPoint
    {
        [STAThread]
        static void Main(string[] args)
        {
            CommandLine.Parser.Default.ParseArguments<AppArguments>(args)
                .WithParsed(args =>
                {
                    if (args.Language is not null)
                    {
                        try
                        {
                            Strings.Culture = new CultureInfo(args.Language);
                            var r = new ResourceManager(typeof(Strings));
                        }
                        catch { }
                    }
                });

            var app = new App();
            app.InitializeComponent();
            app.Run();
        }
    }
}
