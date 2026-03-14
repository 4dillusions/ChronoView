/*
4di .NET ChronoView application
Copyright (c) 2025 by 4D Illusions. All rights reserved.
Released under the terms of the GNU General Public License version 3 or later.
*/

using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;

namespace App4di.Dotnet.ChronoView.AvaloniaUI.Helpers;

internal static class BitmapCache
{
    private static readonly ConcurrentDictionary<string, Task<Bitmap?>> FullImageCache = new(StringComparer.OrdinalIgnoreCase);
    private static readonly ConcurrentDictionary<string, Task<Bitmap?>> ThumbnailCache = new(StringComparer.OrdinalIgnoreCase);

    public static Task<Bitmap?> GetFullImageAsync(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
            return Task.FromResult<Bitmap?>(null);

        return FullImageCache.GetOrAdd(path, static p => Task.Run(() => LoadFullBitmap(p)));
    }

    public static Task<Bitmap?> GetThumbnailAsync(string path, int decodeWidth)
    {
        if (string.IsNullOrWhiteSpace(path))
            return Task.FromResult<Bitmap?>(null);

        var safeWidth = Math.Max(1, decodeWidth);
        var key = $"{safeWidth}:{path}";
        return ThumbnailCache.GetOrAdd(key, static k => Task.Run(() => LoadThumbnailBitmap(k)));
    }

    private static Bitmap? LoadFullBitmap(string path)
    {
        try
        {
            return new Bitmap(path);
        }
        catch
        {
            return null;
        }
    }

    private static Bitmap? LoadThumbnailBitmap(string key)
    {
        var separatorIndex = key.IndexOf(':');
        if (separatorIndex <= 0)
            return null;

        if (!int.TryParse(key.AsSpan(0, separatorIndex), out var decodeWidth))
            return null;

        var path = key[(separatorIndex + 1)..];

        try
        {
            using var stream = File.OpenRead(path);
            return Bitmap.DecodeToWidth(stream, decodeWidth, BitmapInterpolationMode.MediumQuality);
        }
        catch
        {
            return null;
        }
    }
}
