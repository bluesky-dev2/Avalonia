﻿using System;
using System.Linq;
using System.Reactive.Linq;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;

namespace Avalonia.Controls.Notifications
{
    public class NotificationCard : ContentControl
    {
        private bool _isClosed;
        private bool _isClosing;

        static NotificationCard()
        {
            IsClosedProperty.Changed.AddClassHandler<NotificationCard>(IsClosedChanged);
        }

        public NotificationCard()
        {
            this.GetObservable(ContentProperty)
                .OfType<Notification>()
                .Subscribe(x =>
                {
                    switch (x.Type)
                    {
                        case NotificationType.Error:
                            PseudoClasses.Add(":error");
                            break;

                        case NotificationType.Information:
                            PseudoClasses.Add(":information");
                            break;

                        case NotificationType.Success:
                            PseudoClasses.Add(":success");
                            break;

                        case NotificationType.Warning:
                            PseudoClasses.Add(":warning");
                            break;
                    }
                });
        }

        /// <summary>
        /// Determines if the notification is already closing.
        /// </summary>
        public bool IsClosing
        {
            get { return _isClosing; }
            private set { SetAndRaise(IsClosingProperty, ref _isClosing, value); }
        }

        public static readonly DirectProperty<NotificationCard, bool> IsClosingProperty =
            AvaloniaProperty.RegisterDirect<NotificationCard, bool>(nameof(IsClosing), o => o.IsClosing);

        /// <summary>
        /// Determines if the notification is closed.
        /// </summary>
        public bool IsClosed
        {
            get { return _isClosed; }
            set { SetAndRaise(IsClosedProperty, ref _isClosed, value); }
        }

        /// <summary>
        /// Defines the <see cref="IsClosed"/> property.
        /// </summary>
        public static readonly DirectProperty<NotificationCard, bool> IsClosedProperty =
            AvaloniaProperty.RegisterDirect<NotificationCard, bool>(nameof(IsClosed), o => o.IsClosed, (o, v) => o.IsClosed = v);

        /// <summary>
        /// Defines the <see cref="NotificationCloseInvoked"/> event.
        /// </summary>
        public static readonly RoutedEvent<RoutedEventArgs> NotificationCloseInvokedEvent =
            RoutedEvent.Register<NotificationCard, RoutedEventArgs>(nameof(NotificationCloseInvoked), RoutingStrategies.Bubble);

        /// <summary>
        /// Defines the <see cref="NotificationClosed"/> event.
        /// </summary>
        public static readonly RoutedEvent<RoutedEventArgs> NotificationClosedEvent =
            RoutedEvent.Register<NotificationCard, RoutedEventArgs>(nameof(NotificationClosed), RoutingStrategies.Bubble);

        /// <summary>
        /// Raised when notification close event is invoked.
        /// </summary>
        public event EventHandler<RoutedEventArgs> NotificationCloseInvoked
        {
            add { AddHandler(NotificationCloseInvokedEvent, value); }
            remove { RemoveHandler(NotificationCloseInvokedEvent, value); }
        }

        public event EventHandler<RoutedEventArgs> NotificationClosed
        {
            add { AddHandler(NotificationClosedEvent, value); }
            remove { RemoveHandler(NotificationClosedEvent, value); }
        }

        public static bool GetCloseOnClick(NotificationCard obj)
        {
            return (bool)obj.GetValue(CloseOnClickProperty);
        }

        public static void SetCloseOnClick(NotificationCard obj, bool value)
        {
            obj.SetValue(CloseOnClickProperty, value);
        }

        public static readonly AvaloniaProperty CloseOnClickProperty =
          AvaloniaProperty.RegisterAttached<Button, bool>("CloseOnClick", typeof(NotificationCard), validate: CloseOnClickChanged);

        private static bool CloseOnClickChanged(Button button, bool value)
        {
            if (value)
            {
                button.Click += Button_Click;
            }
            else
            {
                button.Click -= Button_Click;
            }

            return true;
        }

        private static void Button_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as ILogical;
            var notification = btn.GetLogicalAncestors().OfType<NotificationCard>().FirstOrDefault();
            notification?.Close();
        }

        protected override void OnTemplateApplied(TemplateAppliedEventArgs e)
        {
            base.OnTemplateApplied(e);
            var closeButton = this.FindControl<Button>("PART_CloseButton");
            if (closeButton != null)
                closeButton.Click += OnCloseButtonOnClick;

        }

        private void OnCloseButtonOnClick(object sender, RoutedEventArgs args)
        {
            var button = sender as Button;
            if (button == null)
                return;

            button.Click -= OnCloseButtonOnClick;
            Close();
        }

        public void Close()
        {
            if (IsClosing)
            {
                return;
            }

            IsClosing = true;

            RaiseEvent(new RoutedEventArgs(NotificationCloseInvokedEvent));
        }

        private static void IsClosedChanged(NotificationCard target, AvaloniaPropertyChangedEventArgs arg2)
        {
            if (!target.IsClosing & !target.IsClosed)
            {
                return;
            }

            target.RaiseEvent(new RoutedEventArgs(NotificationClosedEvent));
        }
    }
}
