using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AppRunner.Models;
using CommunityToolkit.Mvvm.ComponentModel;

namespace AppRunner.ViewModels
{
    public partial class ApplicationsPageModel : ObservableObject
    {
        [ObservableProperty]
        private bool _isAddApplicationDialogOpen;

        public ObservableCollection<RunApp> Applications { get; } = new();




    }
}
