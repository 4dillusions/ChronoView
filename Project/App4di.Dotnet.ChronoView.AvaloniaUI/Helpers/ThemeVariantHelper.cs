/*
4di .NET ChronoView application
Copyright (c) 2025 by 4D Illusions. All rights reserved.
Released under the terms of the GNU General Public License version 3 or later.
*/

using Avalonia.Styling;

namespace App4di.Dotnet.ChronoView.AvaloniaUI.Helpers;

public static class ThemeVariantHelper
{
    public static ThemeVariant FromString(string? theme)
    {
        return theme?.ToLowerInvariant() switch
        {
            "dark" => ThemeVariant.Dark,
            "light" => ThemeVariant.Light,
            "default" => ThemeVariant.Default,
            _ => ThemeVariant.Default
        };
    }
}