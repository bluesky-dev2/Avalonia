using System;
using System.Windows.Input;
using Avalonia.Controls.Utils;
using Avalonia.Data.Core;
using Avalonia.Input;

namespace Avalonia.Controls
{
    public class HotKeyManager
    {
        public static readonly AttachedProperty<KeyGesture> HotKeyProperty
            = AvaloniaProperty.RegisterAttached<Control, KeyGesture>("HotKey", typeof(HotKeyManager));

        class HotkeyCommandWrapper : ICommand
        {
            public HotkeyCommandWrapper(ICommandSource control)
            {
                CommandSource = control;
            }

            public readonly ICommandSource CommandSource;

            private ICommand GetCommand() => CommandSource.Command;

            public bool CanExecute(object parameter) => GetCommand()?.CanExecute(parameter) ?? false;

            public void Execute(object parameter) => GetCommand()?.Execute(parameter);

#pragma warning disable 67 // Event not used
            public event EventHandler CanExecuteChanged;
#pragma warning restore 67
        }


        class Manager
        {
            private readonly IControl _control;
            private TopLevel _root;
            private IDisposable _parentSub;
            private IDisposable _hotkeySub;
            private IDisposable _commandParameterChangedSubscriber;
            private KeyGesture _hotkey;
            private readonly HotkeyCommandWrapper _wrapper;
            private KeyBinding _binding;

            public Manager(IControl control)
            {
                _control = control;
                _wrapper = new HotkeyCommandWrapper(_control as ICommandSource);
            }

            public void Init()
            {
                _hotkeySub = _control.GetObservable(HotKeyProperty).Subscribe(OnHotkeyChanged);
                _parentSub = AncestorFinder.Create<TopLevel>(_control).Subscribe(OnParentChanged);
            }

            private void OnParentChanged(TopLevel control)
            {
                Unregister();
                _root = control;
                Register();
            }

            private void OnHotkeyChanged(KeyGesture hotkey)
            {
                if (hotkey == null)
                    //Subscription will be recreated by static property watcher
                    Stop();
                else
                {
                    Unregister();
                    _hotkey = hotkey;
                    Register();
                }
            }

            void Unregister()
            {
                if (_root != null && _binding != null)
                    _root.KeyBindings.Remove(_binding);
                _commandParameterChangedSubscriber?.Dispose();
                _binding = null;
            }

            void Register()
            {
                if (_root != null && _hotkey != null)
                {
                    _binding = new KeyBinding() {Gesture = _hotkey, Command = _wrapper};
                    _commandParameterChangedSubscriber = _binding.Bind(KeyBinding.CommandParameterProperty
                        , ExpressionObserver.Create(((ICommandSource)_control), o => o.CommandParameter));
                    _root.KeyBindings.Add(_binding);
                }
            }

            void Stop()
            {
                Unregister();
                _parentSub.Dispose();
                _hotkeySub.Dispose();
            }
        }

        static HotKeyManager()
        {
            HotKeyProperty.Changed.Subscribe(args =>
            {
                var control = args.Sender as IControl;
                if (args.OldValue != null || control == null || !(control is ICommandSource)) 
                    return;
                new Manager(control).Init();
            });
        }
        public static void SetHotKey(AvaloniaObject target, KeyGesture value) => target.SetValue(HotKeyProperty, value);
        public static KeyGesture GetHotKey(AvaloniaObject target) => target.GetValue(HotKeyProperty);
    }
}
