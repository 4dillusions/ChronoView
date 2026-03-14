/*
4di .NET ChronoView application
Copyright (c) 2025 by 4D Illusions. All rights reserved.
Released under the terms of the GNU General Public License version 3 or later.
*/

using App4di.Dotnet.ChronoView.AvaloniaUI.View;
using Avalonia.Controls;
using Avalonia.Platform;
using FW4di.Dotnet.Core.DependencyInjection;

namespace App4di.Dotnet.ChronoView.AvaloniaUI;

public partial class MainWindow : Window
{
    public UserControl HomeViewControl { get; set; }
    public UserControl SettingsViewControl { get; set; }
    
    public MainWindow(IDIManager diManager)
    {
        HomeViewControl = diManager.GetDependency<HomeView>();
        SettingsViewControl = diManager.GetDependency<SettingsView>();
        
        InitializeComponent();
        using var iconStream = AssetLoader.Open(new("avares://App4di.Dotnet.ChronoView.AvaloniaUI/Assets/ChronoViewLogo.png"));
        Icon = new WindowIcon(iconStream);
    }
}
