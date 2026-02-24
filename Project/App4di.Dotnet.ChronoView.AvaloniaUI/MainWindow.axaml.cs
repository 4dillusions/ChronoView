/*
4di .NET ChronoView application
Copyright (c) 2025 by 4D Illusions. All rights reserved.
Released under the terms of the GNU General Public License version 3 or later.
*/

using App4di.Dotnet.ChronoView.AvaloniaUI.View;
using Avalonia.Controls;
using FW4di.Dotnet.Core.DependencyInjection;

namespace App4di.Dotnet.ChronoView.AvaloniaUI;

public partial class MainWindow : Window
{
    public UserControl SettingsViewControl { get; set; }
    
    public MainWindow(IDIManager diManager)
    {
        SettingsViewControl = diManager.GetDependency<SettingsView>();
        
        InitializeComponent();
    }
}