using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;

namespace AppRunner.Models
{
    public abstract class RunApp : ObservableObject
    {
        public abstract Task StartAsync();
    }
}
