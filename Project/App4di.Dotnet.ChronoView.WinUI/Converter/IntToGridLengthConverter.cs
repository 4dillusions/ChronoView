/*
4di .NET ChronoView application
Copyright (c) 2025 by 4D Illusions. All rights reserved.
Released under the terms of the GNU General Public License version 3 or later.
*/

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using System;

namespace App4di.Dotnet.ChronoView.WinUI.Converter;

public class IntToGridLengthConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is int i)
            return new GridLength(i);

        return GridLength.Auto;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        if (value is GridLength gl)
            return (int)gl.Value;

        return 0;
    }
}
