/*
4di .NET ChronoView application
Copyright (c) 2025 by 4D Illusions. All rights reserved.
Released under the terms of the GNU General Public License version 3 or later.
*/

using System;
using Avalonia.Controls;
using App4di.Dotnet.ChronoView.Infrastructure.ViewModel;

namespace App4di.Dotnet.ChronoView.AvaloniaUI.View;

public partial class SettingsView : UserControl
{
    public SettingsViewModel ViewModel { get; }  
    
    public SettingsView(SettingsViewModel viewModel)
    {
        ViewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));     
        DataContext = ViewModel;
        
        InitializeComponent();
    }
}