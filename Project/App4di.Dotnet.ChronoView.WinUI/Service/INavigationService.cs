/*
4di .NET ChronoView application
Copyright (c) 2025 by 4D Illusions. All rights reserved.
Released under the terms of the GNU General Public License version 3 or later.
*/

using Microsoft.UI.Xaml.Controls;
using System;

namespace App4di.Dotnet.ChronoView.WinUI.Service;

public interface INavigationService : ICloneable
{
    Frame Nav { get; set; }

    void NavigateTo<T>() where T : Page;
}
