using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using AppRunner.Controls;
using EleCho.WpfSuite.Controls;

namespace AppRunner.Utilities
{
    public static class MessageUtils
    {
        public static void ShowDialogMessage(string title, string message)
        {
            if (DialogLayer.GetDialogLayer(Application.Current.MainWindow) is not DialogLayer dialogLayer)
            {
                throw new InvalidOperationException("Can not find Dialog layer");
            }

            dialogLayer.Push(new Dialog()
            {
                Content = new SimpleMessageToast()
                {
                    MaxWidth = 450,
                    Title = title,
                    Message = message,
                }
            });
        }
    }
}
