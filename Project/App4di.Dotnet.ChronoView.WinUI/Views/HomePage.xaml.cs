/*
4di .NET ChronoView application
Copyright (c) 2025 by 4D Illusions. All rights reserved.
Released under the terms of the GNU General Public License version 3 or later.
*/

using App4di.Dotnet.ChronoView.Infrastructure.DTOs;
using App4di.Dotnet.ChronoView.Infrastructure.Helpers;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using System;
using System.Collections.ObjectModel;

namespace App4di.Dotnet.ChronoView.WinUI.Views;

public sealed partial class HomePage : Page
{
    private double currentAngle = 0;

    public HomePage()
    {
        InitializeComponent();

        this.Loaded += (s, e) =>
        {
            if (Timeline != null)
            {
                Timeline.Items = new ObservableCollection<TimelineItemDTO>
                {
                    new TimelineItemDTO { Timestamp = DateTime.Now.AddMinutes(-30), ImageName = "test1.jpg" },
                    new TimelineItemDTO { Timestamp = DateTime.Now.AddMinutes(-20), ImageName = "test2.jpg" },
                    new TimelineItemDTO { Timestamp = DateTime.Now.AddMinutes(-10), ImageName = "test3.jpg" },
                    new TimelineItemDTO { Timestamp = DateTime.Now, ImageName = "test4.jpg" }
                };

                Timeline.RedrawTimeline();
            }
        };
    }

    private void RotateBtn_Click(object sender, RoutedEventArgs e)
    {
        var animation = new DoubleAnimation
        {
            From = currentAngle,
            To = currentAngle + 90,
            Duration = new Duration(TimeSpan.FromMilliseconds(300)),
            EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseInOut }
        };

        currentAngle += 90;
        if (currentAngle >= 360)
            currentAngle = 0;

        Storyboard.SetTarget(animation, ImgRotate);
        Storyboard.SetTargetProperty(animation, "Angle");

        var storyboard = new Storyboard();
        storyboard.Children.Add(animation);
        storyboard.Begin();
    }

    private void ZoomIn_Click(object sender, RoutedEventArgs e)
    {
        var current = ImageScroller.ZoomFactor;
        var target = Math.Min(current * SettingsManager.ZoomStep, SettingsManager.MaxZoom);

        ZoomTo(target);
    }

    private void ZoomOut_Click(object sender, RoutedEventArgs e)
    {
        var current = ImageScroller.ZoomFactor;
        var target = Math.Max(current / SettingsManager.ZoomStep, SettingsManager.MinZoom);

        ZoomTo(target);
    }

    private void ZoomTo(float targetZoom)
    {
        ImageScroller.ChangeView(ImageScroller.HorizontalOffset, ImageScroller.VerticalOffset, targetZoom);
    }

    private void ResetZoom_Click(object sender, RoutedEventArgs e)
    {
        ZoomTo(1);
    }
}
