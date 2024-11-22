using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommandLine;

namespace AppRunner.Models
{
    class AppArguments
    {
        [Option('n', "language")]
        public string? Language { get; set; }
    }
}
