/*
4di .NET ChronoView application
Copyright (c) 2025 by 4D Illusions. All rights reserved.
Released under the terms of the GNU General Public License version 3 or later.
*/

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using System;

namespace App4di.Dotnet.ChronoView.WinUI.Views;

public sealed partial class HomePage : Page
{
    private double currentAngle = 0;

    private const float MinZoom = 0.5f;
    private const float MaxZoom = 8.0f;
    private const float ZoomStep = 1.25f;

    public HomePage()
    {
        InitializeComponent();
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
        var target = Math.Min(current * ZoomStep, MaxZoom);

        ZoomTo(target);
    }

    private void ZoomOut_Click(object sender, RoutedEventArgs e)
    {
        var current = ImageScroller.ZoomFactor;
        var target = Math.Max(current / ZoomStep, MinZoom);

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
