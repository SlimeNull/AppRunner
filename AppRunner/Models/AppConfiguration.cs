using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;

namespace AppRunner.Models
{
    public partial class AppConfiguration : ObservableObject
    {
        [ObservableProperty]
        private RunApp[]? _applications;

        [ObservableProperty]
        private RunEnvironment[]? _environments;


    }
}
