﻿using System;
using System.Collections.Generic;
using Avalonia.Collections;
using Avalonia.Controls.Platform;
using Avalonia.Platform;

#nullable enable

namespace Avalonia.Controls
{
    public sealed class TrayIcons : AvaloniaList<TrayIcon>
    {
    }

    public class TrayIcon : AvaloniaObject, IDataContextProvider
    {
        private readonly ITrayIconImpl _impl;

        private TrayIcon(ITrayIconImpl impl)
        {
            _impl = impl;

            _impl.SetIsVisible(IsVisible);
        }

        public TrayIcon () : this(PlatformManager.CreateTrayIcon())
        {
        }

        static TrayIcon ()
        {
            TrayIconsProperty.Changed.Subscribe(args =>
            {
                if (args.Sender is Application application)
                {
                    if(args.OldValue.Value != null)
                    {
                        RemoveIcons(args.OldValue.Value);
                    }

                    if(args.NewValue.Value != null)
                    {
                        args.NewValue.Value.CollectionChanged += Icons_CollectionChanged;
                    }
                }
            });
        }

        private static void Icons_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            
        }

        private static void RemoveIcons (IEnumerable<TrayIcon> icons)
        {
            foreach(var icon in icons)
            {
                icon.Remove();
            }
        }


        public static readonly AttachedProperty<TrayIcons> TrayIconsProperty
            = AvaloniaProperty.RegisterAttached<TrayIcon, Application, TrayIcons>("TrayIcons");

        /// <summary>
        /// Defines the <see cref="DataContext"/> property.
        /// </summary>
        public static readonly StyledProperty<object?> DataContextProperty =
            StyledElement.DataContextProperty.AddOwner<Application>();

        /// <summary>
        /// Defines the <see cref="Icon"/> property.
        /// </summary>
        public static readonly StyledProperty<WindowIcon> IconProperty =
            Window.IconProperty.AddOwner<TrayIcon>();


        public static readonly StyledProperty<string?> ToolTipTextProperty =
            AvaloniaProperty.Register<TrayIcon, string?>(nameof(ToolTipText));

        /// <summary>
        /// Defines the <see cref="IsVisibleProperty"/> property.
        /// </summary>
        public static readonly StyledProperty<bool> IsVisibleProperty =
            Visual.IsVisibleProperty.AddOwner<TrayIcon>();

        public static void SetTrayIcons(AvaloniaObject o, TrayIcons trayIcons) => o.SetValue(TrayIconsProperty, trayIcons);

        public static TrayIcons GetTrayIcons(AvaloniaObject o) => o.GetValue(TrayIconsProperty);

        /// <summary>
        /// Removes the notify icon from the taskbar notification area.
        /// </summary>
        public void Remove()
        {

        }


        public new ITrayIconImpl PlatformImpl => _impl;


        /// <summary>
        /// Gets or sets the Applications's data context.
        /// </summary>
        /// <remarks>
        /// The data context property specifies the default object that will
        /// be used for data binding.
        /// </remarks>
        public object? DataContext
        {
            get => GetValue(DataContextProperty);
            set => SetValue(DataContextProperty, value);
        }

        /// <summary>
        /// Gets or sets the icon of the TrayIcon.
        /// </summary>
        public WindowIcon Icon
        {
            get => GetValue(IconProperty);
            set => SetValue(IconProperty, value);
        }

        /// <summary>
        /// Gets or sets the tooltip text of the TrayIcon.
        /// </summary>
        public string? ToolTipText
        {
            get => GetValue(ToolTipTextProperty);
            set => SetValue(ToolTipTextProperty, value);
        }

        /// <summary>
        /// Gets or sets the visibility of the TrayIcon.
        /// </summary>
        public bool IsVisible
        {
            get => GetValue(IsVisibleProperty);
            set => SetValue(IsVisibleProperty, value);
        }

        protected override void OnPropertyChanged<T>(AvaloniaPropertyChangedEventArgs<T> change)
        {
            base.OnPropertyChanged(change);

            if(change.Property == IconProperty)
            {
                _impl.SetIcon(Icon.PlatformImpl);
            }
            else if (change.Property == IsVisibleProperty)
            {
                _impl.SetIsVisible(change.NewValue.GetValueOrDefault<bool>());
            }
            else if (change.Property == ToolTipTextProperty)
            {
                _impl.SetToolTipText(change.NewValue.GetValueOrDefault<string?>());
            }
        }
    }
}
