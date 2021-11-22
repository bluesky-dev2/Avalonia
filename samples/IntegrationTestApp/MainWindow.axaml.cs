using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;

namespace IntegrationTestApp
{
    public class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            InitializeViewMenu();
            this.AttachDevTools();
            AddHandler(Button.ClickEvent, OnButtonClick);
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        private void InitializeViewMenu()
        {
            var mainTabs = this.FindControl<TabControl>("MainTabs");
            var viewMenu = (NativeMenuItem)NativeMenu.GetMenu(this).Items[1];

            foreach (TabItem tabItem in mainTabs.Items)
            {
                var menuItem = new NativeMenuItem
                {
                    Header = (string)tabItem.Header!,
                    IsChecked = tabItem.IsSelected,
                    ToggleType = NativeMenuItemToggleType.Radio,
                };

                menuItem.Click += (s, e) => tabItem.IsSelected = true;
                viewMenu.Menu.Items.Add(menuItem);
            }
        }

        private void MenuClicked(object? sender, RoutedEventArgs e)
        {
            var clickedMenuItemTextBlock = this.FindControl<TextBlock>("ClickedMenuItem");
            clickedMenuItemTextBlock.Text = ((MenuItem)sender!).Header.ToString();
        }

        private void OnButtonClick(object? sender, RoutedEventArgs e)
        {
            var source = e.Source as Button;

            if (source?.Name == "ComboBoxSelectionClear")
                this.FindControl<ComboBox>("ComboBox").SelectedIndex = -1;
            if (source?.Name == "ComboBoxSelectFirst")
                this.FindControl<ComboBox>("ComboBox").SelectedIndex = 0;
        }
    }
}
