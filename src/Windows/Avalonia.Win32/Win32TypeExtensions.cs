﻿using static Avalonia.Win32.Interop.UnmanagedMethods;

namespace Avalonia.Win32
{
    public static class Win32TypeExtensions
    {
        internal static PixelRect ToPixelRect(this RECT rect)
        {
            return new PixelRect(rect.left, rect.top, rect.right - rect.left,
                    rect.bottom - rect.top);
        }
    }
}
