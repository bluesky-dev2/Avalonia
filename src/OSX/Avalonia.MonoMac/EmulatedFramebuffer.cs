﻿using System;
using System.Runtime.InteropServices;
using Avalonia.Platform;
using Avalonia.Threading;
using MonoMac.AppKit;
using MonoMac.CoreGraphics;

namespace Avalonia.MonoMac
{
    class EmulatedFramebuffer : ILockedFramebuffer
    {
        private readonly TopLevelImpl.TopLevelView _view;
        private readonly CGSize _logicalSize;
        private readonly bool _isDeferred;
        private readonly IUnmanagedBlob _blob;

        [DllImport("libc")]
        static extern void memset(IntPtr p, int c, IntPtr size);

        public EmulatedFramebuffer(TopLevelImpl.TopLevelView view)
        {
            _view = view;

            _isDeferred = !Dispatcher.UIThread.CheckAccess();
            _logicalSize = _view.LogicalSize;
            var pixelSize = _view.PixelSize;
            Size = new PixelSize((int)pixelSize.Width, (int)pixelSize.Height);
            RowBytes = Size.Width * 4;
            Dpi = new Vector(96 * pixelSize.Width / _logicalSize.Width, 96 * pixelSize.Height / _logicalSize.Height);
            Format = PixelFormat.Rgba8888;
            var size = Size.Height * RowBytes;
            _blob = AvaloniaLocator.Current.GetService<IRuntimePlatform>().AllocBlob(size);
            memset(Address, 0, new IntPtr(size));
        }
        
        public void Dispose()
        {
            if (_blob.IsDisposed)
                return;
            var nfo = (int) CGBitmapFlags.ByteOrder32Big | (int) CGImageAlphaInfo.PremultipliedLast;
            CGImage image = null;
            try
            {
                using (var colorSpace = CGColorSpace.CreateDeviceRGB())
                using (var bContext = new CGBitmapContext(Address, Size.Width, Size.Height, 8, Size.Width * 4,
                    colorSpace, (CGImageAlphaInfo)nfo))
                    image = bContext.ToImage();
                lock (_view.SyncRoot)
                {
                    if(!_isDeferred)
                    {
                        using (var nscontext = NSGraphicsContext.CurrentContext)
                        using (var context = nscontext.GraphicsPort)
                        {
                            context.SetFillColor(255, 255, 255, 255);
                            context.FillRect(new CGRect(default(CGPoint), _view.LogicalSize));
                            context.TranslateCTM(0, _view.LogicalSize.Height - _logicalSize.Height);
                            context.DrawImage(new CGRect(default(CGPoint), _logicalSize), image);
                            context.Flush();
                            nscontext.FlushGraphics();
                        }
                    }
                }
            }
            finally
            {
                if (image != null)
                {
                    if (!_isDeferred)
                        image.Dispose();
                    else
                        _view.SetBackBufferImage(new SavedImage(image, _logicalSize));
                }
                _blob.Dispose();
            }


        }

        public IntPtr Address => _blob.Address;
        public PixelSize Size { get; }
        public int RowBytes { get; }
        public Vector Dpi { get; }
        public PixelFormat Format { get; }
    }

    class SavedImage : IDisposable
    {
        public CGImage Image { get; private set; }
        public CGSize LogicalSize { get; }

        public SavedImage(CGImage image, CGSize logicalSize)
        {
            Image = image;
            LogicalSize = logicalSize;
        }

        public void Dispose()
        {
            Image?.Dispose();
            Image = null;
        }
    }
}
