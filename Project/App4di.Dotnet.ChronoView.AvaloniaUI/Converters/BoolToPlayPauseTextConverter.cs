/*
4di .NET ChronoView application
Copyright (c) 2025 by 4D Illusions. All rights reserved.
Released under the terms of the GNU General Public License version 3 or later.
*/

using System;
using Avalonia.Data.Converters;

namespace App4di.Dotnet.ChronoView.AvaloniaUI.Converters;

public sealed class BoolToPlayPauseTextConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, System.Globalization.CultureInfo culture)
        => value is bool b && b ? "⏸" : "▶";

    public object? ConvertBack(object? value, Type targetType, object? parameter, System.Globalization.CultureInfo culture)
        => throw new NotSupportedException();
}
