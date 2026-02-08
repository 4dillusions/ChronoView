/*
4di .NET ChronoView application
Copyright (c) 2025 by 4D Illusions. All rights reserved.
Released under the terms of the GNU General Public License version 3 or later.
*/

using App4di.Dotnet.ChronoView.Infrastructure.Service;
using Microsoft.UI.Xaml;
using System;
using DIBindings = App4di.Dotnet.ChronoView.WinUI.Service.DIBindings;

namespace App4di.Dotnet.ChronoView.WinUI;

public partial class App : Application
{
    DIBindings diBindings = new DIBindings();
    ISettingsService settings;

    public App()
    {
        InitializeComponent();

        diBindings.BindAllDepencies();
        settings = diBindings.GetDependency<ISettingsService>();
        RequestedTheme = Enum.Parse<ApplicationTheme>(settings.Theme);
    }

    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        var mainWindow = diBindings.GetDependency<MainWindow>();
        mainWindow.Closed += (s, e) => settings.SaveSettings();
        mainWindow.Activate();
    }
}
