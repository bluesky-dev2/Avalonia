﻿using OpenQA.Selenium.Appium;
using Xunit;

namespace Avalonia.IntegrationTests.Appium
{
    [Collection("Default")]
    public class MenuTests
    {
        private AppiumDriver<AppiumWebElement> _session;

        public MenuTests(TestAppFixture fixture) => _session = fixture.Session;

        [Fact]
        public void File()
        {
            var fileMenu = _session.FindElementByAccessibilityId("FileMenu");

            Assert.Equal("File", fileMenu.Text);
        }

        [PlatformFact(SkipOnOSX = true)]
        public void Open()
        {
            var fileMenu = _session.FindElementByAccessibilityId("FileMenu");
            fileMenu.Click();

            var openMenu = fileMenu.FindElementByAccessibilityId("OpenMenu");
            Assert.Equal("Ctrl+O", openMenu.GetAttribute("AcceleratorKey"));
        }
    }
}
