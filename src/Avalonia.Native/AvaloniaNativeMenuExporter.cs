﻿using System;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls.Platform;
using Avalonia.Dialogs;
using Avalonia.Native.Interop;
using Avalonia.Threading;

namespace Avalonia.Native
{
    class AvaloniaNativeMenuExporter : ITopLevelNativeMenuExporter
    {
        private IAvaloniaNativeFactory _factory;
        private bool _resetQueued;
        private bool _exported = false;
        private IAvnWindow _nativeWindow;
        private NativeMenu _menu;
        private IAvnAppMenu _nativeMenu;
        private IDisposable _disposable;

        public AvaloniaNativeMenuExporter(IAvnWindow nativeWindow, IAvaloniaNativeFactory factory)
        {
            _factory = factory;
            _nativeWindow = nativeWindow;

            DoLayoutReset();
        }

        public AvaloniaNativeMenuExporter(IAvaloniaNativeFactory factory)
        {
            _factory = factory;

            DoLayoutReset();
        }

        public bool IsNativeMenuExported => _exported;

        public event EventHandler OnIsNativeMenuExportedChanged;

        public void SetNativeMenu(NativeMenu menu)
        {
            _menu = menu == null ? new NativeMenu() : menu;

            DoLayoutReset();
        }

        private static NativeMenu CreateDefaultAppMenu()
        {
            var result = new NativeMenu();

            var aboutItem = new NativeMenuItem
            {
                Header = "About Avalonia",
            };

            aboutItem.Clicked += async (sender, e) =>
            {
                var dialog = new AboutAvaloniaDialog();

                var mainWindow = (Application.Current.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)?.MainWindow;

                await dialog.ShowDialog(mainWindow);
            };

            result.Add(aboutItem);

            return result;
        }

        void DoLayoutReset()
        {
            _resetQueued = false;

            if (_nativeWindow is null)
            {
                var appMenu = NativeMenu.GetMenu(Application.Current);

                if (appMenu == null)
                {
                    appMenu = CreateDefaultAppMenu();
                    SetMenu(appMenu);
                }
            }
            else
            {
                if (_menu != null)
                {
                    SetMenu(_nativeWindow, _menu);
                }
            }

            _exported = true;
        }

        internal void QueueReset()
        {
            if (_resetQueued)
                return;
            _resetQueued = true;
            Dispatcher.UIThread.Post(DoLayoutReset, DispatcherPriority.Background);
        }

        private void SetMenu(NativeMenu menu)
        {
            if (_nativeMenu is null)
            {
                _nativeMenu = _factory.ObtainAppMenu();

                if (_nativeMenu is null)
                {
                    _nativeMenu = _factory.CreateMenu();
                }
            }

            var menuItem = menu.Parent;

            var appMenuHolder = menuItem?.Parent;

            if (menu.Parent is null)
            {
                menuItem = new NativeMenuItem();
            }

            if (appMenuHolder is null)
            {
                appMenuHolder = new NativeMenu();

                appMenuHolder.Add(menuItem);
            }

            menuItem.Menu = menu;

            _disposable?.Dispose();
            _disposable = _nativeMenu.Update(this, _factory, appMenuHolder);

            _factory.SetAppMenu(_nativeMenu);
        }

        private void SetMenu(IAvnWindow avnWindow, NativeMenu menu)
        {
            if (_nativeMenu is null)
            {
                _nativeMenu = avnWindow.ObtainMainMenu();

                if (_nativeMenu is null)
                {
                    _nativeMenu = _factory.CreateMenu();
                }
            }

            _disposable?.Dispose();
            _disposable = _nativeMenu.Update(this, _factory, menu);

            avnWindow.SetMainMenu(_nativeMenu);
        }
    }
}
