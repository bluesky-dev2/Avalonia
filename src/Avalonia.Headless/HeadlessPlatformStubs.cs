using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Controls.Platform;
using Avalonia.Input;
using Avalonia.Input.Platform;
using Avalonia.Platform;

namespace Avalonia.Headless
{
    class HeadlessClipboardStub : IClipboard
    {
        private string _text;
        private IDataObject _data;

        public Task<string> GetTextAsync()
        {
            return Task.Run(() => _text);
        }

        public Task SetTextAsync(string text)
        {
            return Task.Run(() => _text = text);
        }

        public Task ClearAsync()
        {
            return Task.Run(() => _text = null);
        }

        public Task SetDataObjectAsync(IDataObject data)
        {
            return Task.Run(() => _data = data);
        }

        public Task<string[]> GetFormatsAsync()
        {
            throw new NotImplementedException();
        }

        public async Task<object> GetDataAsync(string format)
        {
            return await Task.Run(() => _data);
        }
    }

    class HeadlessCursorFactoryStub : IStandardCursorFactory
    {
        
        public IPlatformHandle GetCursor(StandardCursorType cursorType)
        {
            return new PlatformHandle(new IntPtr((int)cursorType), "STUB");
        }
    }

    class HeadlessPlatformSettingsStub : IPlatformSettings
    {
        public Size DoubleClickSize { get; } = new Size(2, 2);
        public TimeSpan DoubleClickTime { get; } = TimeSpan.FromMilliseconds(500);
    }

    class HeadlessSystemDialogsStub : ISystemDialogImpl
    {
        public Task<string[]> ShowFileDialogAsync(FileDialog dialog, Window parent)
        {
            return Task.Run(() => (string[])null);
        }

        public Task<string> ShowFolderDialogAsync(OpenFolderDialog dialog, Window parent)
        {
            return Task.Run(() => (string)null);
        }
    }

    class HeadlessIconLoaderStub : IPlatformIconLoader
    {

        class IconStub : IWindowIconImpl
        {
            public void Save(Stream outputStream)
            {
                
            }
        }
        public IWindowIconImpl LoadIcon(string fileName)
        {
            return new IconStub();
        }

        public IWindowIconImpl LoadIcon(Stream stream)
        {
            return new IconStub();
        }

        public IWindowIconImpl LoadIcon(IBitmapImpl bitmap)
        {
            return new IconStub();
        }
    }

    class HeadlessScreensStub : IScreenImpl
    {
        public int ScreenCount { get; } = 1;

        public IReadOnlyList<Screen> AllScreens { get; } = new[]
        {
            new Screen(1, new PixelRect(0, 0, 1920, 1280),
                new PixelRect(0, 0, 1920, 1280), true),
        };
    }
}
