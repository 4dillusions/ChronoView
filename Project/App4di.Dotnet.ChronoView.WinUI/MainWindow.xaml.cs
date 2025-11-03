/*
4di .NET ChronoView application
Copyright (c) 2025 by 4D Illusions. All rights reserved.
Released under the terms of the GNU General Public License version 3 or later.
*/

using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using System;
using Windows.Graphics;
using Muxc = Microsoft.UI.Xaml.Controls;

namespace App4di.Dotnet.ChronoView.WinUI;

public sealed partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        var appWindow = this.AppWindow;

        int width = 1280;
        int height = 768;

        var displayArea = DisplayArea.GetFromWindowId(appWindow.Id, DisplayAreaFallback.Primary);
        var workArea = displayArea.WorkArea;

        int x = workArea.X + (workArea.Width - width) / 2;
        int y = workArea.Y + (workArea.Height - height) / 2;
        appWindow.MoveAndResize(new RectInt32(x, y, width, height));

        if (appWindow.Presenter is OverlappedPresenter p)
        {
            p.PreferredMinimumWidth = width;
            p.PreferredMinimumHeight = height;
        }

        ContentFrame.Navigate(typeof(Views.HomePage));
    }

    private void NavigationView_SelectionChanged(Muxc.NavigationView sender, Muxc.NavigationViewSelectionChangedEventArgs args)
    {
        if (args.SelectedItem is Muxc.NavigationViewItem nvi && nvi.Tag is string tag)
        {
            Type page = tag switch
            {
                "home" => typeof(Views.HomePage),
                "tech" => typeof(Views.TechnologyPage),
                "contact" => typeof(Views.ContactPage),
                "about" => typeof(Views.AboutPage),
                _ => typeof(Views.HomePage)
            };

            if (ContentFrame.CurrentSourcePageType != page)
                ContentFrame.Navigate(page);
        }
    }
}
