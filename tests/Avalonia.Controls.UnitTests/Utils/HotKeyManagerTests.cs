using Moq;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Templates;
using Avalonia.Input;
using Avalonia.Platform;
using Avalonia.Styling;
using Xunit;
using System;
using Avalonia.Input.Raw;
using Factory = System.Func<int, System.Action<object>, Avalonia.Controls.Window, Avalonia.AvaloniaObject>;

namespace Avalonia.Controls.UnitTests.Utils
{
    public class HotKeyManagerTests
    {
        [Fact]
        public void HotKeyManager_Should_Register_And_Unregister_Key_Binding()
        {
            using (AvaloniaLocator.EnterScope())
            {
                var styler = new Mock<Styler>();

                AvaloniaLocator.CurrentMutable
                    .Bind<IWindowingPlatform>().ToConstant(new WindowingPlatformMock())
                    .Bind<IStyler>().ToConstant(styler.Object);

                var gesture1 = new KeyGesture(Key.A, KeyModifiers.Control);
                var gesture2 = new KeyGesture(Key.B, KeyModifiers.Control);

                var tl = new Window();
                var button = new Button();
                tl.Content = button;
                tl.Template = CreateWindowTemplate();
                tl.ApplyTemplate();
                tl.Presenter.ApplyTemplate();

                HotKeyManager.SetHotKey(button, gesture1);

                Assert.Equal(gesture1, tl.KeyBindings[0].Gesture);

                HotKeyManager.SetHotKey(button, gesture2);
                Assert.Equal(gesture2, tl.KeyBindings[0].Gesture);

                tl.Content = null;
                tl.Presenter.ApplyTemplate();

                Assert.Empty(tl.KeyBindings);

                tl.Content = button;
                tl.Presenter.ApplyTemplate();

                Assert.Equal(gesture2, tl.KeyBindings[0].Gesture);

                HotKeyManager.SetHotKey(button, null);
                Assert.Empty(tl.KeyBindings);

            }
        }

        [Theory]
        [MemberData(nameof(ElementsFactory))]
        public void HotKeyManager_Should_Use_CommandParameter(string factoryName, Factory factory)
        {
            using (AvaloniaLocator.EnterScope())
            {
                var styler = new Mock<Styler>();
                var target = new KeyboardDevice();
                var commandResult = 0;
                var expectedParameter = 1;
                AvaloniaLocator.CurrentMutable
                    .Bind<IWindowingPlatform>().ToConstant(new WindowingPlatformMock())
                    .Bind<IStyler>().ToConstant(styler.Object);

                var gesture = new KeyGesture(Key.A, KeyModifiers.Control);

                var action = new Action<object>(parameter =>
                {
                    if (parameter is int value)
                    {
                        commandResult = value;
                    }
                });

                var root = new Window();
                var element = factory(expectedParameter, action, root);

                root.Template = CreateWindowTemplate();
                root.ApplyTemplate();
                root.Presenter.ApplyTemplate();

                HotKeyManager.SetHotKey(element, gesture);

                target.ProcessRawEvent(new RawKeyEventArgs(target,
                    0,
                    root,
                    RawKeyEventType.KeyDown,
                    Key.A,
                    RawInputModifiers.Control));

                Assert.True(expectedParameter == commandResult, $"{factoryName} HotKey did not carry the CommandParameter.");
            }
        }

        public static TheoryData<string, Factory> ElementsFactory =>
            new TheoryData<string, Factory>()
            {
                {nameof(Button), MakeButton},
                {nameof(MenuItem),MakeMenu},
            };

        private static AvaloniaObject MakeMenu(int expectedParameter, Action<object> action, Window root)
        {
            var menuitem = new MenuItem()
            {
                Command = new Command(action),
                CommandParameter = expectedParameter,
            };
            var rootMenu = new Menu();

            rootMenu.Items = new[] { menuitem };

            root.Content = rootMenu;
            return menuitem;
        }

        private static AvaloniaObject MakeButton(int expectedParameter, Action<object> action, Window root)
        {
            var button = new Button()
            {
                Command = new Command(action),
                CommandParameter = expectedParameter,
            };

            root.Content = button;
            return button;
        }

        private FuncControlTemplate CreateWindowTemplate()
        {
            return new FuncControlTemplate<Window>((parent, scope) =>
            {
                return new ContentPresenter
                {
                    Name = "PART_ContentPresenter",
                    [~ContentPresenter.ContentProperty] = parent[~ContentControl.ContentProperty],
                }.RegisterInNameScope(scope);
            });
        }

        class Command : System.Windows.Input.ICommand
        {
            private readonly Action<object> _execeute;

#pragma warning disable 67 // Event not used
            public event EventHandler CanExecuteChanged;
#pragma warning restore 67 // Event not used

            public Command(Action<object> execeute)
            {
                _execeute = execeute;
            }
            public bool CanExecute(object parameter) => true;

            public void Execute(object parameter) => _execeute?.Invoke(parameter);
        }
    }
}
