using System;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Logging.Serilog;
using Avalonia.Media;
using Avalonia.Platform;
using Serilog;

namespace ControlCatalog
{
    internal class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            // TODO: Make this work with GTK/Skia/Cairo depending on command-line args
            // again.
            AvaloniaLocator.CurrentMutable.Bind<IFontFamilyCache>().ToConstant(new FontFamilyCache());
            BuildAvaloniaApp().Start<MainWindow>();
        }

        /// <summary>
        /// This method is needed for IDE previewer infrastructure
        /// </summary>
        public static AppBuilder BuildAvaloniaApp()
            => AppBuilder.Configure<App>().LogToDebug().UsePlatformDetect();

        private static void ConfigureAssetAssembly(AppBuilder builder)
        {
            AvaloniaLocator.CurrentMutable
                .GetService<IAssetLoader>()
                .SetDefaultAssembly(typeof(App).Assembly);           
        }
    }
}
