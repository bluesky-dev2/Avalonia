﻿// Copyright (c) The Avalonia Project. All rights reserved.
// Licensed under the MIT license. See licence.md file in the project root for full license information.

using System;

namespace Avalonia.Platform
{
    public interface IMacOSTopLevelPlatformHandle
    {
        IntPtr NSView { get; }
        IntPtr GetNSViewRetained();
        IntPtr NSWindow { get; }
        IntPtr GetNSWindowRetained();
    }
}
