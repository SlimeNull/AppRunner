using System.Windows;
using System.Windows.Markup;

[assembly: ThemeInfo(
    ResourceDictionaryLocation.None,            //where theme specific resource dictionaries are located
                                                //(used if a resource is not found in the page,
                                                // or application resource dictionaries)
    ResourceDictionaryLocation.SourceAssembly   //where the generic resource dictionary is located
                                                //(used if a resource is not found in the page,
                                                // app, or any theme specific resource dictionaries)
)]

[assembly: XmlnsDefinition("https://schemas.elecho.dev/wpfsuite", "EleCho.WpfSuite.Controls", AssemblyName = "EleCho.WpfSuite.Controls")]
[assembly: XmlnsDefinition("https://schemas.elecho.dev/wpfsuite", "EleCho.WpfSuite.Helpers", AssemblyName = "EleCho.WpfSuite.Helpers")]
[assembly: XmlnsDefinition("https://schemas.elecho.dev/wpfsuite", "EleCho.WpfSuite.Input", AssemblyName = "EleCho.WpfSuite.Input")]
[assembly: XmlnsDefinition("https://schemas.elecho.dev/wpfsuite", "EleCho.WpfSuite.Layouts", AssemblyName = "EleCho.WpfSuite.Layouts")]
[assembly: XmlnsDefinition("https://schemas.elecho.dev/wpfsuite", "EleCho.WpfSuite.Markup", AssemblyName = "EleCho.WpfSuite.Markup")]
[assembly: XmlnsDefinition("https://schemas.elecho.dev/wpfsuite", "EleCho.WpfSuite.Media", AssemblyName = "EleCho.WpfSuite.Media")]
[assembly: XmlnsDefinition("https://schemas.elecho.dev/wpfsuite", "EleCho.WpfSuite.Media.Transition", AssemblyName = "EleCho.WpfSuite.Media")]
[assembly: XmlnsDefinition("https://schemas.elecho.dev/wpfsuite", "EleCho.WpfSuite.ValueConverters", AssemblyName = "EleCho.WpfSuite.ValueConverters")]
