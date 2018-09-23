﻿using System;
using Avalonia.Native.Interop;
using Avalonia.Platform;

namespace Avalonia.Native
{
    public class PopupImpl : WindowBaseImpl, IPopupImpl
    {
        IAvnPopup _native;

        public PopupImpl(IAvaloniaNativeFactory factory)
        {
            using (var e = new PopupEvents(this))
                Init(_native = factory.CreatePopup(e));
        }

        class PopupEvents : WindowBaseEvents, IAvnWindowEvents
        {
            readonly PopupImpl _parent;

            public PopupEvents(PopupImpl parent) : base(parent)
            {
                _parent = parent;
            }
        }
    }
}
