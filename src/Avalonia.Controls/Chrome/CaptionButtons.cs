﻿using System;
using System.Collections.Generic;
using System.Reactive.Disposables;
using System.Text;
using Avalonia.Controls.Primitives;
using Avalonia.VisualTree;

namespace Avalonia.Controls.Chrome
{
    public class CaptionButtons : TemplatedControl
    {
        private CompositeDisposable _disposables;
        private Window _hostWindow;

        public CaptionButtons(Window hostWindow)
        {
            _hostWindow = hostWindow;
        }

        public void Attach()
        {
            if (_disposables == null)
            {
                var layer = ChromeOverlayLayer.GetOverlayLayer(_hostWindow);

                layer.Children.Add(this);

                _disposables = new CompositeDisposable
                {
                    _hostWindow.GetObservable(Window.WindowDecorationMarginsProperty)
                    .Subscribe(x =>
                    {
                        Height = x.Top;
                    }),


                    _hostWindow.GetObservable(Window.ExtendClientAreaTitleBarHeightHintProperty)
                    .Subscribe(x => InvalidateSize()),

                    _hostWindow.GetObservable(Window.OffScreenMarginProperty)
                    .Subscribe(x => InvalidateSize()),

                    _hostWindow.GetObservable(Window.WindowStateProperty)
                    .Subscribe(x =>
                    {
                        PseudoClasses.Set(":minimized", x == WindowState.Minimized);
                        PseudoClasses.Set(":normal", x == WindowState.Normal);
                        PseudoClasses.Set(":maximized", x == WindowState.Maximized);
                        PseudoClasses.Set(":fullscreen", x == WindowState.FullScreen);
                    })
                };
            }
        }

        void InvalidateSize ()
        {
            Margin = new Thickness(1, _hostWindow.OffScreenMargin.Top, 1, 1);
            Height = _hostWindow.WindowDecorationMargins.Top - _hostWindow.OffScreenMargin.Top;
        }

        public void Detach()
        {
            if (_disposables != null)
            {
                var layer = ChromeOverlayLayer.GetOverlayLayer(_hostWindow);

                layer.Children.Remove(this);

                _disposables.Dispose();
                _disposables = null;
            }
        }

        protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
        {
            base.OnApplyTemplate(e);

            var closeButton = e.NameScope.Find<Panel>("PART_CloseButton");
            var restoreButton = e.NameScope.Find<Panel>("PART_RestoreButton");
            var minimiseButton = e.NameScope.Find<Panel>("PART_MinimiseButton");
            var fullScreenButton = e.NameScope.Find<Panel>("PART_FullScreenButton");

            closeButton.PointerPressed += (sender, e) => _hostWindow.Close();
            restoreButton.PointerPressed += (sender, e) => _hostWindow.WindowState = _hostWindow.WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
            minimiseButton.PointerPressed += (sender, e) => _hostWindow.WindowState = WindowState.Minimized;
            fullScreenButton.PointerPressed += (sender, e) => _hostWindow.WindowState = _hostWindow.WindowState == WindowState.FullScreen ? WindowState.Normal : WindowState.FullScreen;
        }
    }
}
