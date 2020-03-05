﻿using System;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Text;
using Avalonia.Data.Converters;
using Avalonia.Input;

namespace Avalonia.Controls.Converters
{
    public class PlatformKeyGestureConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is null)
            {
                return null;
            }
            else if (value is KeyGesture gesture && targetType == typeof(string))
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    return ToString(gesture, "Win");
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    return ToString(gesture, "Super");
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    return ToOSXString(gesture);
                }
                else
                {
                    return gesture.ToString();
                }
            }
            else
            {
                throw new NotSupportedException();
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        private static string ToString(KeyGesture gesture, string meta)
        {
            var s = new StringBuilder();

            static void Plus(StringBuilder s)
            {
                if (s.Length > 0)
                {
                    s.Append("+");
                }
            }

            if (gesture.KeyModifiers.HasFlagCustom(KeyModifiers.Control))
            {
                s.Append("Ctrl");
            }

            if (gesture.KeyModifiers.HasFlagCustom(KeyModifiers.Shift))
            {
                Plus(s);
                s.Append("Shift");
            }

            if (gesture.KeyModifiers.HasFlagCustom(KeyModifiers.Alt))
            {
                Plus(s);
                s.Append("Alt");
            }

            if (gesture.KeyModifiers.HasFlagCustom(KeyModifiers.Meta))
            {
                Plus(s);
                s.Append(meta);
            }

            Plus(s);
            s.Append(ToString(gesture.Key));

            return s.ToString();
        }

        private static string ToOSXString(KeyGesture gesture)
        {
            var s = new StringBuilder();

            if (gesture.KeyModifiers.HasFlagCustom(KeyModifiers.Control))
            {
                s.Append('⌃');
            }

            if (gesture.KeyModifiers.HasFlagCustom(KeyModifiers.Alt))
            {
                s.Append('⌥');
            }

            if (gesture.KeyModifiers.HasFlagCustom(KeyModifiers.Shift))
            {
                s.Append('⇧');
            }

            if (gesture.KeyModifiers.HasFlagCustom(KeyModifiers.Meta))
            {
                s.Append('⌘');
            }

            s.Append(ToOSXString(gesture.Key));

            return s.ToString();
        }

        private static string ToString(Key key)
        {
            return key switch
            {
                Key.Add => "+",
                Key.Back => "Backspace",
                Key.D0 => "0",
                Key.D1 => "1",
                Key.D2 => "2",
                Key.D3 => "3",
                Key.D4 => "4",
                Key.D5 => "5",
                Key.D6 => "6",
                Key.D7 => "7",
                Key.D8 => "8",
                Key.D9 => "9",
                Key.Divide => "/",
                Key.OemBackslash => "\\",
                Key.OemCloseBrackets => "]",
                Key.OemComma => ",",
                Key.OemMinus => "-",
                Key.OemOpenBrackets => "[",
                Key.OemPeriod=> ".",
                Key.OemPipe => "|",
                Key.OemPlus => "+",
                Key.OemQuestion => "/",
                Key.OemQuotes => "\"",
                Key.OemSemicolon => ";",
                Key.OemTilde => "`",
                Key.Subtract => "-",
                _ => key.ToString(),
            };
        }

        private static string ToOSXString(Key key)
        {
            return key switch
            {
                Key.Back => "⌫",
                Key.Down => "↓",
                Key.End => "↘",
                Key.Escape => "⎋",
                Key.Home => "↖",
                Key.Left => "←",
                Key.Return => "↩",
                Key.PageDown => "⇞",
                Key.PageUp => "⇟",
                Key.Right => "→",
                Key.Space => "␣",
                Key.Tab => "⇥",
                Key.Up => "↑",
                _ => ToString(key),
            };
        }
    }
}
