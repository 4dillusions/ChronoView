/*
4di .NET ChronoView application
Copyright (c) 2025 by 4D Illusions. All rights reserved.
Released under the terms of the GNU General Public License version 3 or later.
*/

using System;
using System.Collections.Concurrent;
using Avalonia.Data.Converters;
using Avalonia.Media.Imaging;

namespace App4di.Dotnet.ChronoView.AvaloniaUI.Converters;

public sealed class ImagePathToBitmapConverter : IValueConverter
{
    private static readonly ConcurrentDictionary<string, Bitmap> Cache = new(StringComparer.OrdinalIgnoreCase);

    public object? Convert(object? value, Type targetType, object? parameter, System.Globalization.CultureInfo culture)
    {
        if (value is not string path || string.IsNullOrWhiteSpace(path))
            return null;

        // Very small, pragmatic cache – avoids re-decoding the same thumbnail repeatedly.
        // If you end up loading huge folders, consider adding an LRU policy.
        if (Cache.TryGetValue(path, out var cached))
            return cached;

        try
        {
            var bmp = new Bitmap(path);
            Cache[path] = bmp;
            return bmp;
        }
        catch
        {
            return null;
        }
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, System.Globalization.CultureInfo culture)
        => throw new NotSupportedException();
}
