﻿using System.Threading;
using OpenQA.Selenium;
using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.Mac;
using Xunit;

namespace Avalonia.IntegrationTests.Appium
{
    [Collection("Default")]
    public class ComboBoxTests
    {
        private readonly AppiumDriver<AppiumWebElement> _session;

        public ComboBoxTests(TestAppFixture fixture)
        {
            _session = fixture.Session;

            var tabs = _session.FindElementByAccessibilityId("MainTabs");
            var tab = tabs.FindElementByName("ComboBox");
            tab.Click();
        }

        [Fact]
        public void Can_Change_Selection_Using_Mouse()
        {
            var comboBox = _session.FindElementByAccessibilityId("ComboBox");

            _session.FindElementByAccessibilityId("ComboBoxSelectFirst").Click();
            Assert.Equal("Item 0", comboBox.Text);

            comboBox.Click();
            _session.FindElementByName("Item 1").SendClick();

            Assert.Equal("Item 1", comboBox.Text);
        }

        [Fact]
        public void Can_Change_Selection_From_Unselected_Using_Mouse()
        {
            var comboBox = _session.FindElementByAccessibilityId("ComboBox");

            _session.FindElementByAccessibilityId("ComboBoxSelectionClear").Click();
            Assert.Equal(string.Empty, comboBox.Text);

            comboBox.Click();
            _session.FindElementByName("Item 0").SendClick();

            Assert.Equal("Item 0", comboBox.Text);
        }

        [Fact]
        public void Can_Change_Selection_With_Keyboard()
        {
            var comboBox = _session.FindElementByAccessibilityId("ComboBox");

            _session.FindElementByAccessibilityId("ComboBoxSelectFirst").Click();
            Assert.Equal("Item 0", comboBox.Text);

            comboBox.SendKeys(Keys.LeftAlt + Keys.ArrowDown);
            comboBox.SendKeys(Keys.ArrowDown);

            var item = _session.FindElementByName("Item 1");
            item.SendKeys(Keys.Enter);

            Assert.Equal("Item 1", comboBox.Text);
        }

        [Fact]
        public void Can_Change_Selection_With_Keyboard_From_Unselected()
        {
            var comboBox = _session.FindElementByAccessibilityId("ComboBox");

            _session.FindElementByAccessibilityId("ComboBoxSelectionClear").Click();
            Assert.Equal(string.Empty, comboBox.Text);

            comboBox.SendKeys(Keys.LeftAlt + Keys.ArrowDown);
            comboBox.SendKeys(Keys.ArrowDown);

            var item = _session.FindElementByName("Item 0");
            item.SendKeys(Keys.Enter);

            Assert.Equal("Item 0", comboBox.Text);
        }

        [Fact]
        public void Can_Cancel_Keyboard_Selection_With_Escape()
        {
            var comboBox = _session.FindElementByAccessibilityId("ComboBox");

            _session.FindElementByAccessibilityId("ComboBoxSelectionClear").Click();
            Assert.Equal(string.Empty, comboBox.Text);

            comboBox.SendKeys(Keys.LeftAlt + Keys.ArrowDown);
            comboBox.SendKeys(Keys.ArrowDown);

            var item = _session.FindElementByName("Item 0");
            item.SendKeys(Keys.Escape);

            Assert.Equal(string.Empty, comboBox.Text);
        }
    }
}
