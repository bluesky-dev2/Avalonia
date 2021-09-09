﻿using System;
using Avalonia.Controls;
using Avalonia.Controls.Platform;

#nullable enable

namespace Avalonia.Platform
{
    public interface ITrayIconImpl
    {
        /// <summary>
        /// Sets the icon of this tray icon.
        /// </summary>
        void SetIcon(IWindowIconImpl? icon);

        /// <summary>
        /// Sets the icon of this tray icon.
        /// </summary>
        void SetToolTipText(string? text);

        /// <summary>
        /// Sets if the tray icon is visible or not.
        /// </summary>
        void SetIsVisible (bool visible);

        INativeMenuExporter? MenuExporter { get; }
    }
}
