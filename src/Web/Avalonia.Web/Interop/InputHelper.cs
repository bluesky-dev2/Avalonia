﻿using System;
using System.Runtime.InteropServices.JavaScript;
using System.Threading.Tasks;

namespace Avalonia.Web.Interop;

internal static partial class InputHelper
{
    [JSImport("InputHelper.subscribeKeyboardEvents", "avalonia.ts")]
    public static partial void SubscribeKeyboardEvents(
        JSObject htmlElement,
        [JSMarshalAs<JSType.Function<JSType.String, JSType.String, JSType.Number, JSType.Boolean>>]
        Func<string, string, int, bool> keyDown,
        [JSMarshalAs<JSType.Function<JSType.String, JSType.String, JSType.Number, JSType.Boolean>>]
        Func<string, string, int, bool> keyUp,
        [JSMarshalAs<JSType.Function<JSType.Object, JSType.Boolean>>]
        Func<JSObject, bool> onInput,
        [JSMarshalAs<JSType.Function<JSType.Object, JSType.Boolean>>]
        Func<JSObject, bool> onCompositionStart,
        [JSMarshalAs<JSType.Function<JSType.Object, JSType.Boolean>>]
        Func<JSObject, bool> onCompositionUpdate,
        [JSMarshalAs<JSType.Function<JSType.Object, JSType.Boolean>>]
        Func<JSObject, bool> onCompositionEnd);

    [JSImport("InputHelper.subscribePointerEvents", "avalonia.ts")]
    public static partial void SubscribePointerEvents(
        JSObject htmlElement,
        [JSMarshalAs<JSType.Function<JSType.Object, JSType.Boolean>>]
        Func<JSObject, bool> pointerMove,
        [JSMarshalAs<JSType.Function<JSType.Object, JSType.Boolean>>]
        Func<JSObject, bool> pointerDown,
        [JSMarshalAs<JSType.Function<JSType.Object, JSType.Boolean>>]
        Func<JSObject, bool> pointerUp,
        [JSMarshalAs<JSType.Function<JSType.Object, JSType.Boolean>>]
        Func<JSObject, bool> wheel);
        

    [JSImport("InputHelper.subscribeInputEvents", "avalonia.ts")]
    public static partial void SubscribeInputEvents(
        JSObject htmlElement,
        [JSMarshalAs<JSType.Function<JSType.String, JSType.Boolean>>]
        Func<string, bool> input);


    [JSImport("InputHelper.clearInput", "avalonia.ts")]
    public static partial void ClearInputElement(JSObject htmlElement);

    [JSImport("InputHelper.isInputElement", "avalonia.ts")]
    public static partial void IsInputElement(JSObject htmlElement);

    [JSImport("InputHelper.focusElement", "avalonia.ts")]
    public static partial void FocusElement(JSObject htmlElement);

    [JSImport("InputHelper.setCursor", "avalonia.ts")]
    public static partial void SetCursor(JSObject htmlElement, string kind);

    [JSImport("InputHelper.hide", "avalonia.ts")]
    public static partial void HideElement(JSObject htmlElement);

    [JSImport("InputHelper.show", "avalonia.ts")]
    public static partial void ShowElement(JSObject htmlElement);


    [JSImport("navigator.clipboard.readText")]
    public static partial Task<string> ReadClipboardTextAsync();

    [JSImport("navigator.clipboard.writeText")]
    public static partial Task WriteClipboardTextAsync(string text);
}
