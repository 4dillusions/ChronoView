/*
4di .NET ChronoView application
Copyright (c) 2025 by 4D Illusions. All rights reserved.
Released under the terms of the GNU General Public License version 3 or later.
*/

using Avalonia;
using Avalonia.Controls;

namespace App4di.Dotnet.ChronoView.AvaloniaUI.Controls;

public partial class CardIcon : UserControl
{
    public static readonly StyledProperty<string?> TextProperty =
        AvaloniaProperty.Register<CardIcon, string?>(nameof(Text));

    public string? Text
    {
        get => GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public CardIcon()
    {
        InitializeComponent();
    }
}