﻿// This source file is adapted from the WinUI project.
// (https://github.com/microsoft/microsoft-ui-xaml)
//
// Licensed to The Avalonia Project under MIT License, courtesy of The .NET Foundation.

using System;
using System.Collections.Generic;
using System.Text;

namespace Avalonia.Controls.Repeaters
{
    public class LayoutContext : AvaloniaObject
    {
        public object LayoutState { get; set; }

        protected virtual object LayoutStateCore { get; set; }
    }
}
