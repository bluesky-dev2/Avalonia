using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using RenderDemo.ViewModels;
using ReactiveUI;

namespace RenderDemo
{
    public class MainWindow : Window
    {
        public MainWindow()
        {
            this.InitializeComponent();
            this.AttachDevTools();

            var vm = new MainWindowViewModel();
            vm.WhenAnyValue(x => x.DrawDirtyRects).Subscribe(x => Renderer.DrawDirtyRects = x);
            vm.WhenAnyValue(x => x.DrawFps).Subscribe(x => Renderer.DrawFps = x);
            this.DataContext = vm;
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
