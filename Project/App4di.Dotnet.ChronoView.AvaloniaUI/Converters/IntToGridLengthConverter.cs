/*
4di .NET ChronoView application
Copyright (c) 2025 by 4D Illusions. All rights reserved.
Released under the terms of the GNU General Public License version 3 or later.
*/

using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data.Converters;

namespace App4di.Dotnet.ChronoView.AvaloniaUI.Converters;

public sealed class IntToGridLengthConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, System.Globalization.CultureInfo culture)
    {
        if (value is null)
            return GridLength.Auto;

        if (value is int i)
            return new GridLength(Math.Max(0, i), GridUnitType.Pixel);

        if (value is double d)
            return new GridLength(Math.Max(0, d), GridUnitType.Pixel);

        if (value is float f)
            return new GridLength(Math.Max(0, f), GridUnitType.Pixel);

        return GridLength.Auto;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, System.Globalization.CultureInfo culture)
        => throw new NotSupportedException();
}
