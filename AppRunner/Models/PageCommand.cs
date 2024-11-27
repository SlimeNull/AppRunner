using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace AppRunner.Models
{
    public record class PageCommand(string Icon, string Description, ICommand TargetCommand, FontFamily FontFamily)
    {
        const string FontAwesomeResourceKey = "FontAwesome";
        const string FontAwesomeBrandsResourceKey = "FontAwesomeBrands";

        static FontFamily? _fontAwesome;
        static FontFamily? _fontAwesomeBrands;

        public static FontFamily FontAwesome => _fontAwesome ??= (FontFamily)Application.Current.FindResource(FontAwesomeResourceKey);
        public static FontFamily FontAwesomeBrands => _fontAwesomeBrands ??= (FontFamily)Application.Current.FindResource(FontAwesomeBrandsResourceKey);

        public PageCommand(string icon, string description, ICommand targetCommand) :
            this(icon, description, targetCommand, FontAwesome)
        {

        }
    }
}
