﻿using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using MiniMvvm;

namespace ControlCatalog.ViewModels
{
    public class ApplicationViewModel : ViewModelBase
    {
        public ApplicationViewModel()
        {
            ExitCommand = MiniCommand.Create(() =>
            {
                if(Application.Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime lifetime)
                {
                    lifetime.Shutdown();
                }
            });
        }

        public MiniCommand ExitCommand { get; }
    }
}
