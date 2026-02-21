/*
4di .NET ChronoView application
Copyright (c) 2025 by 4D Illusions. All rights reserved.
Released under the terms of the GNU General Public License version 3 or later.
*/

using System;
using System.Globalization;
using System.Resources;
using Avalonia.Markup.Xaml;

namespace App4di.Dotnet.ChronoView.AvaloniaUI.Localization;

public sealed class TrExtension : MarkupExtension
{
    #region Properties
    private static readonly ResourceManager Rm =
        new("App4di.Dotnet.ChronoView.AvaloniaUI.Localization.Resources",
            typeof(TrExtension).Assembly);

    string Key { get; } = "";
    #endregion

    #region CTor
    public TrExtension() { }
    public TrExtension(string key) => Key = key;
    #endregion
    
    #region IValueConverter
    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        if (string.IsNullOrWhiteSpace(Key))
            return "";

        var s = Rm.GetString(Key, CultureInfo.CurrentUICulture);
        return string.IsNullOrEmpty(s) ? $"[{Key}]" : s;
    }
    #endregion
}