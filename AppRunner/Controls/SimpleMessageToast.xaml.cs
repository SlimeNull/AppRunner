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
using EleCho.WpfSuite.Controls;

namespace AppRunner.Controls
{
    /// <summary>
    /// Interaction logic for SimpleMessageToast.xaml
    /// </summary>
    public partial class SimpleMessageToast : UserControl
    {
        public SimpleMessageToast()
        {
            InitializeComponent();
        }



        public string Title
        {
            get { return (string)GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }


        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register(nameof(Title), typeof(string), typeof(SimpleMessageToast), new PropertyMetadata(string.Empty));

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            if (Dialog.GetDialog(this) is Dialog dialog)
            {
                dialog.IsOpen = false;
            }
        }
    }
}
