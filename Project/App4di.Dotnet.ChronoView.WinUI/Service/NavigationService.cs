/*
4di .NET ChronoView application
Copyright (c) 2025 by 4D Illusions. All rights reserved.
Released under the terms of the GNU General Public License version 3 or later.
*/

using FW4di.Dotnet.Core.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using System;

namespace App4di.Dotnet.ChronoView.WinUI.Service;

public class NavigationService(IDIManager di) : INavigationService, ICloneable
{
    private readonly IDIManager di = di ?? throw new ArgumentNullException(nameof(di));

    public Frame Nav { get; set; }

    public object Clone()
    {
        return MemberwiseClone();
    }

    public void NavigateTo<T>() where T : Page
    {
        var page = di.GetDependency<T>();
        Nav.Content = page;
    }
}
