using System;
using System.Runtime.InteropServices;
using OpenQA.Selenium.Appium;

namespace Avalonia.IntegrationTests.Win32
{
    internal static class ElementExtensions
    {
        public static string GetName(this AppiumWebElement element) => GetAttribute(element, "Name", "title");

        public static string GetAttribute(AppiumWebElement element, string windows, string macOS)
        {
            return element.GetAttribute(RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? windows : macOS);
        }
    }
}
