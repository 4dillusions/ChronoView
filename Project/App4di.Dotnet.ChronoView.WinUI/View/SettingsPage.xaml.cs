/*
4di .NET ChronoView application
Copyright (c) 2025 by 4D Illusions. All rights reserved.
Released under the terms of the GNU General Public License version 3 or later.
*/

using App4di.Dotnet.ChronoView.Infrastructure.ViewModel;
using FW4di.Dotnet.Core.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using System;

namespace App4di.Dotnet.ChronoView.WinUI.View;

public sealed partial class SettingsPage : Page
{
    public SettingsViewModel ViewModel { get; }
    public SettingsPage(SettingsViewModel viewModel, IDIManager _)
    {
        ViewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
        DataContext = ViewModel;

        InitializeComponent();
    }
}
