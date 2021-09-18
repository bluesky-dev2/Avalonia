using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reactive.Disposables;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Avalonia.Controls.Platform;
using Tmds.DBus;

[assembly: InternalsVisibleTo(Tmds.DBus.Connection.DynamicAssemblyName)]

namespace Avalonia.FreeDesktop.DBusSystemTray
{
    public class SNIDBus : IDisposable
    {
        public SNIDBus()
        {
        }

        private static int trayinstanceID = 0;
        private IStatusNotifierWatcher _snw;
        private string _sysTraySrvName;
        private StatusNotifierItem _statusNotifierItem;
        public INativeMenuExporter NativeMenuExporter;

        private static int GetTID()
        {
            return trayinstanceID = new Random().Next(0, 100);
        }

        public async void Initialize()
        {
            var con = DBusHelper.Connection;

            _snw = con.CreateProxy<IStatusNotifierWatcher>("org.kde.StatusNotifierWatcher",
                "/StatusNotifierWatcher");

            var x = Process.GetCurrentProcess().Id;
            var y = GetTID();

            _sysTraySrvName = $"org.kde.StatusNotifierItem-{x}-{y}";
            _statusNotifierItem = new StatusNotifierItem();

            await con.RegisterObjectAsync(_statusNotifierItem);

            await con.RegisterServiceAsync(_sysTraySrvName);

            await _snw.RegisterStatusNotifierItemAsync(_sysTraySrvName);

            NativeMenuExporter = _statusNotifierItem.NativeMenuExporter;

        }

        public async void Dispose()
        {
            var con = DBusHelper.Connection;

            if (await con.UnregisterServiceAsync(_sysTraySrvName))
            {
                con.UnregisterObject(_statusNotifierItem);
            }
        }

        public void SetIcon(Pixmap pixmap)
        {
            _statusNotifierItem.SetIcon(pixmap);
        }
    }

    internal class StatusNotifierItem : IStatusNotifierItem
    {
        private event Action<PropertyChanges> OnPropertyChange;
        public event Action OnTitleChanged;
        public event Action OnIconChanged;
        public event Action OnAttentionIconChanged;
        public event Action OnOverlayIconChanged;


        public Action NewToolTipAsync;
        public ObjectPath ObjectPath { get; }

        readonly StatusNotifierItemProperties props;

        public StatusNotifierItem()
        {
            var ID = Guid.NewGuid().ToString().Replace("-", "");
            ObjectPath = new ObjectPath($"/StatusNotifierItem");
            var blankPixmaps = new[] { new Pixmap(0, 0, new byte[] { }), new Pixmap(0, 0, new byte[] { }) };

            var dbusmenuPath = DBusMenuExporter.GenerateDBusMenuObjPath;

            NativeMenuExporter = DBusMenuExporter.TryCreateDetachedNativeMenu(dbusmenuPath);

            props = new StatusNotifierItemProperties
            {
                Menu = "/MenuBar", // Needs a dbus menu somehow
                ItemIsMenu = false,
                ToolTip = new ToolTip("", blankPixmaps, "Avalonia Test Tray", ""),
                Category = "",
                Title = "Avalonia Test Tray",
                Status = "Avalonia Test Tray",
                Id = "Avalonia Test Tray",
            };
        }

        public INativeMenuExporter NativeMenuExporter;

        public async Task ContextMenuAsync(int X, int Y)
        {
        }

        public async Task ActivateAsync(int X, int Y)
        {
            // OnPropertyChange?.Invoke(new PropertyChanges());
        }

        public async Task SecondaryActivateAsync(int X, int Y)
        {
            //throw new NotImplementedException();5
        }

        public async Task ScrollAsync(int Delta, string Orientation)
        {
        }

        public void InvalidateAll()
        {
            OnTitleChanged?.Invoke();
            OnIconChanged?.Invoke();
            OnOverlayIconChanged?.Invoke();
            OnAttentionIconChanged?.Invoke();
        }

        public async Task<IDisposable> WatchNewTitleAsync(Action handler, Action<Exception> onError = null)
        {
            OnTitleChanged += handler;
            return Disposable.Create(() => OnTitleChanged -= handler);
        }


        public async Task<IDisposable> WatchNewIconAsync(Action handler, Action<Exception> onError = null)
        {
            OnIconChanged += handler;
            return Disposable.Create(() => OnIconChanged -= handler);
        }

        public async Task<IDisposable> WatchNewAttentionIconAsync(Action handler, Action<Exception> onError = null)
        {
            OnAttentionIconChanged += handler;
            return Disposable.Create(() => OnAttentionIconChanged -= handler);
        }


        public async Task<IDisposable> WatchNewOverlayIconAsync(Action handler, Action<Exception> onError = null)
        {
            OnOverlayIconChanged += handler;
            return Disposable.Create(() => OnOverlayIconChanged -= handler);
        }

        public async Task<IDisposable> WatchNewToolTipAsync(Action handler, Action<Exception> onError = null)
        {
            NewToolTipAsync += handler;
            return Disposable.Create(() => NewToolTipAsync -= handler);
        }

        public async Task<IDisposable> WatchNewStatusAsync(Action<string> handler, Action<Exception> onError = null)
        {
            NewStatusAsync += handler;
            return Disposable.Create(() => NewStatusAsync -= handler);
        }

        public async Task<object> GetAsync(string prop)
        {
            return prop switch
            {
                "Category" => props.Category,
                "Id" => props.Id,
                "Title" => props.Title,
                "Status" => props.Status,
                "WindowId" => props.WindowId,
                "IconThemePath" => props.IconThemePath,
                "ItemIsMenu" => props.ItemIsMenu,
                "IconName" => props.IconName,
                "IconPixmap" => props.IconPixmap,
                "OverlayIconName" => props.OverlayIconName,
                "OverlayIconPixmap" => props.OverlayIconPixmap,
                "AttentionIconName" => props.AttentionIconName,
                "AttentionIconPixmap" => props.AttentionIconPixmap,
                "AttentionMovieName" => props.AttentionMovieName,
                "ToolTip" => props.ToolTip,
                _ => default
            };
        }

        public async Task<StatusNotifierItemProperties> GetAllAsync()
        {
            return props;
        }

        public Action<string> NewStatusAsync { get; set; }

        public Task SetAsync(string prop, object val) => Task.CompletedTask;

        public async Task<IDisposable> WatchPropertiesAsync(Action<PropertyChanges> handler)
        {
            OnPropertyChange += handler;
            return Disposable.Create(() => OnPropertyChange -= handler);
        }

        public void SetIcon(Pixmap pixmap)
        {
            props.IconPixmap = new[] { pixmap };
            InvalidateAll();
        }
    }

    [DBusInterface("org.kde.StatusNotifierWatcher")]
    internal interface IStatusNotifierWatcher : IDBusObject
    {
        Task RegisterStatusNotifierItemAsync(string Service);
        Task RegisterStatusNotifierHostAsync(string Service);

        Task<IDisposable> WatchStatusNotifierItemRegisteredAsync(Action<string> handler,
            Action<Exception> onError = null);

        Task<IDisposable> WatchStatusNotifierItemUnregisteredAsync(Action<string> handler,
            Action<Exception> onError = null);

        Task<IDisposable> WatchStatusNotifierHostRegisteredAsync(Action handler, Action<Exception> onError = null);
        Task<T> GetAsync<T>(string prop);
        Task<StatusNotifierWatcherProperties> GetAllAsync();
        Task SetAsync(string prop, object val);
        Task<IDisposable> WatchPropertiesAsync(Action<PropertyChanges> handler);
    }

    [Dictionary]
    internal class StatusNotifierWatcherProperties
    {
        public string[] RegisteredStatusNotifierItems;

        public bool IsStatusNotifierHostRegistered;

        public int ProtocolVersion;
    }

    internal static class StatusNotifierWatcherExtensions
    {
        public static Task<string[]> GetRegisteredStatusNotifierItemsAsync(this IStatusNotifierWatcher o) =>
            o.GetAsync<string[]>("RegisteredStatusNotifierItems");

        public static Task<bool> GetIsStatusNotifierHostRegisteredAsync(this IStatusNotifierWatcher o) =>
            o.GetAsync<bool>("IsStatusNotifierHostRegistered");

        public static Task<int> GetProtocolVersionAsync(this IStatusNotifierWatcher o) =>
            o.GetAsync<int>("ProtocolVersion");
    }


    [DBusInterface("org.kde.StatusNotifierItem")]
    interface IStatusNotifierItem : IDBusObject
    {
        Task ContextMenuAsync(int X, int Y);
        Task ActivateAsync(int X, int Y);
        Task SecondaryActivateAsync(int X, int Y);
        Task ScrollAsync(int Delta, string Orientation);
        Task<IDisposable> WatchNewTitleAsync(Action handler, Action<Exception> onError = null);
        Task<IDisposable> WatchNewIconAsync(Action handler, Action<Exception> onError = null);
        Task<IDisposable> WatchNewAttentionIconAsync(Action handler, Action<Exception> onError = null);
        Task<IDisposable> WatchNewOverlayIconAsync(Action handler, Action<Exception> onError = null);
        Task<IDisposable> WatchNewToolTipAsync(Action handler, Action<Exception> onError = null);
        Task<IDisposable> WatchNewStatusAsync(Action<string> handler, Action<Exception> onError = null);
        Task<object> GetAsync(string prop);
        Task<StatusNotifierItemProperties> GetAllAsync();
        Task SetAsync(string prop, object val);
        Task<IDisposable> WatchPropertiesAsync(Action<PropertyChanges> handler);
    }


    [Dictionary]
    internal class StatusNotifierItemProperties
    {
        public string Category;

        public string Id;

        public string Title;

        public string Status;

        public int WindowId;

        public string IconThemePath;

        public ObjectPath Menu;

        public bool ItemIsMenu;

        public string IconName;

        public Pixmap[] IconPixmap;

        public string OverlayIconName;

        public Pixmap[] OverlayIconPixmap;

        public string AttentionIconName;

        public Pixmap[] AttentionIconPixmap;

        public string AttentionMovieName;

        public ToolTip ToolTip;
    }

    public readonly struct ToolTip
    {
        public readonly string First;
        public readonly Pixmap[] Second;
        public readonly string Third;
        public readonly string Fourth;

        public ToolTip(string first, Pixmap[] second, string third, string fourth)
        {
            First = first;
            Second = second;
            Third = third;
            Fourth = fourth;
        }
    }

    public readonly struct Pixmap
    {
        public readonly int Width;
        public readonly int Height;
        public readonly byte[] Data;

        public Pixmap(int width, int height, byte[] data)
        {
            Width = width;
            Height = height;
            Data = data;
        }
    }
}
