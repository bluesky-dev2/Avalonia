using System;
using System.Collections;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Media.Immutable;
using Avalonia.Platform;
using Avalonia.Themes.Fluent;
using ControlCatalog.Models;
using ControlCatalog.Pages;

namespace ControlCatalog
{
    public class MainView : UserControl
    {
        public MainView()
        {
            AvaloniaXamlLoader.Load(this);

            var sideBar = this.FindControl<TabControl>("Sidebar");

            if (AvaloniaLocator.Current.GetService<IRuntimePlatform>().GetRuntimeInfo().IsDesktop)
            {
                IList tabItems = ((IList)sideBar.Items);
                tabItems.Add(new TabItem()
                {
                    Header = "Dialogs",
                    Content = new DialogsPage()
                });
                tabItems.Add(new TabItem()
                {
                    Header = "Screens",
                    Content = new ScreenPage()
                });

            }

            var themes = this.Find<ComboBox>("Themes");
            themes.SelectionChanged += (sender, e) =>
            {
                if (themes.SelectedItem is CatalogTheme theme)
                {
                    var themeStyle = Application.Current.Styles[0];
                    if (theme == CatalogTheme.FluentLight)
                    {
                        if (themeStyle is FluentTheme fluentTheme)
                        {
                            if (fluentTheme.Mode == FluentThemeMode.Dark)
                            {
                                fluentTheme.Mode = FluentThemeMode.Light;
                            }
                        }
                        else
                        {
                            Application.Current.Styles[0] = new FluentTheme(new Uri("avares://ControlCatalog/Styles"));
                            Application.Current.Styles[1] = App.DataGridFluent;
                        }
                    }
                    else if (theme == CatalogTheme.FluentDark)
                    {
                        if (themeStyle is FluentTheme fluentTheme)
                        {
                            if (fluentTheme.Mode == FluentThemeMode.Light)
                            {
                                fluentTheme.Mode = FluentThemeMode.Dark;
                            }
                        }
                        else
                        {
                            Application.Current.Styles[0] = new FluentTheme(new Uri("avares://ControlCatalog/Styles")) { Mode = FluentThemeMode.Dark };
                            Application.Current.Styles[1] = App.DataGridFluent;
                        }
                    }
                    else if (theme == CatalogTheme.DefaultLight)
                    {
                        Application.Current.Styles[0] = App.DefaultLight;
                        Application.Current.Styles[1] = App.DataGridDefault;
                    }
                    else if (theme == CatalogTheme.DefaultDark)
                    {
                        Application.Current.Styles[0] = App.DefaultDark;
                        Application.Current.Styles[1] = App.DataGridDefault;
                    }
                }
            };

            var decorations = this.Find<ComboBox>("Decorations");
            decorations.SelectionChanged += (sender, e) =>
            {
                if (VisualRoot is Window window
                    && decorations.SelectedItem is SystemDecorations systemDecorations)
                {
                    window.SystemDecorations = systemDecorations;
                }
            };

            var transparencyLevels = this.Find<ComboBox>("TransparencyLevels");
            IDisposable backgroundSetter = null, paneBackgroundSetter = null;
            transparencyLevels.SelectionChanged += (sender, e) =>
            {
                backgroundSetter?.Dispose();
                paneBackgroundSetter?.Dispose();
                if (transparencyLevels.SelectedItem is WindowTransparencyLevel selected
                    && selected != WindowTransparencyLevel.None)
                {
                    var semiTransparentBrush = new ImmutableSolidColorBrush(Colors.Gray, 0.5);
                    backgroundSetter = sideBar.SetValue(BackgroundProperty, semiTransparentBrush, Avalonia.Data.BindingPriority.Style);
                    paneBackgroundSetter = sideBar.SetValue(SplitView.PaneBackgroundProperty, semiTransparentBrush, Avalonia.Data.BindingPriority.Style);
                }
            };
        }

        protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
        {
            base.OnAttachedToVisualTree(e);
            var decorations = this.Find<ComboBox>("Decorations");
            if (VisualRoot is Window window)
                decorations.SelectedIndex = (int)window.SystemDecorations;
        }
    }
}
