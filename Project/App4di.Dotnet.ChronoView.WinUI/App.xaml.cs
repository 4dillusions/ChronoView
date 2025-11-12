/*
4di .NET ChronoView application
Copyright (c) 2025 by 4D Illusions. All rights reserved.
Released under the terms of the GNU General Public License version 3 or later.
*/

using App4di.Dotnet.ChronoView.WinUI.Service;
using Microsoft.UI.Xaml;

namespace App4di.Dotnet.ChronoView.WinUI;

public partial class App : Application
{
    DIBindings diBindings = new DIBindings();

    public App()
    {
        InitializeComponent();

        diBindings.BindAllDepencies();
        RequestedTheme = ApplicationTheme.Dark;
    }

    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        diBindings.GetDependency<MainWindow>().Activate();
    }
}
