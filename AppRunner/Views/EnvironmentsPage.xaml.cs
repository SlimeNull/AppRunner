using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using AppRunner.ViewModels;

namespace AppRunner.Views
{
    /// <summary>
    /// Interaction logic for EnvironmentsPage.xaml
    /// </summary>
    public partial class EnvironmentsPage : Page
    {
        public EnvironmentsPage(
            EnvironmentsPageModel viewModel)
        {
            ViewModel = viewModel;
            DataContext = this;
            InitializeComponent();
        }

        public EnvironmentsPageModel ViewModel { get; }
    }
}
