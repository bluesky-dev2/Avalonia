﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls.Notifications;
using Avalonia.Threading;

namespace ControlCatalog.ViewModels
{
    class MainWindowViewModel
    {
        public MainWindowViewModel()
        {
            Dispatcher.UIThread.InvokeAsync(async () =>
            {
                await Task.Delay(5000);

                
                Application.Current.MainWindow.LocalNotificationManager.Show(new NotificationViewModel { Title = "Warning", Message = "Please save your work before closing." });

                await Task.Delay(1500);
                Application.Current.MainWindow.LocalNotificationManager.Show(new NotificationContent { Message = "Test2", Type = NotificationType.Error });

                await Task.Delay(2000);
                Application.Current.MainWindow.LocalNotificationManager.Show(new NotificationContent { Message = "Test3", Type = NotificationType.Warning });

                await Task.Delay(2500);
                Application.Current.MainWindow.LocalNotificationManager.Show(new NotificationContent { Message = "Test4", Type = NotificationType.Success });

                await Task.Delay(500);
                Application.Current.MainWindow.LocalNotificationManager.Show("Test5");

            });
        }
    }
}
