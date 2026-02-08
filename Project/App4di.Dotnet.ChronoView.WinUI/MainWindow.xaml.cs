/*
4di .NET ChronoView application
Copyright (c) 2025 by 4D Illusions. All rights reserved.
Released under the terms of the GNU General Public License version 3 or later.
*/

using App4di.Dotnet.ChronoView.Infrastructure.Service;
using App4di.Dotnet.ChronoView.WinUI.Service;
using App4di.Dotnet.ChronoView.WinUI.View;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using System;
using Windows.Graphics;
using Muxc = Microsoft.UI.Xaml.Controls;

namespace App4di.Dotnet.ChronoView.WinUI;

public sealed partial class MainWindow : Window
{
    private readonly INavigationService navigationService;
    private readonly ISettingsService settings;

    public MainWindow(INavigationService navigationService, ISettingsService settings)
    {
        InitializeComponent();

        var appWindow = this.AppWindow;

        this.settings = settings ?? throw new ArgumentNullException(nameof(settings));
        int width = settings.MinWidth;
        int height = settings.MinHeight;

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

        this.navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
        navigationService.Nav = ContentFrame;
        navigationService.Nav.CacheSize = 10;
    }

    private void NavigationView_SelectionChanged(Muxc.NavigationView sender, Muxc.NavigationViewSelectionChangedEventArgs args)
    {
        if (args.SelectedItem is Muxc.NavigationViewItem nvi && nvi.Tag is string tag)
        {
            Type page = tag switch
            {
                "home" => typeof(HomePage),
                "settings" => typeof(SettingsPage),
                "about" => typeof(AboutPage),
                _ => typeof(HomePage)
            };

            if (navigationService.Nav.CurrentSourcePageType != page)
            {
                var method = typeof(NavigationService).GetMethod("NavigateTo");
                var generic = method?.MakeGenericMethod(page);

                generic?.Invoke(navigationService, null);
            }
        }
    }
}
