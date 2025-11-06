/*
4di .NET ChronoView application
Copyright (c) 2025 by 4D Illusions. All rights reserved.
Released under the terms of the GNU General Public License version 3 or later.
*/

using App4di.Dotnet.ChronoView.Infrastructure.DTO;
using App4di.Dotnet.ChronoView.Infrastructure.ViewModel;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using System;
using System.Collections.ObjectModel;

namespace App4di.Dotnet.ChronoView.WinUI.View;

public sealed partial class HomePage : Page
{
    public HomeViewModel ViewModel { get; }

    public HomePage()
    {
        InitializeComponent();

        ViewModel = new HomeViewModel();
        DataContext = ViewModel;

        Loaded += (s, e) =>
        {
            if (Timeline?.ViewModel != null)
            {
                Timeline.ViewModel.Items = new ObservableCollection<TimelineItemDTO>
                {
                    new TimelineItemDTO { Timestamp = DateTime.Now.AddMinutes(-30), ImageName = "test1.jpg" },
                    new TimelineItemDTO { Timestamp = DateTime.Now.AddMinutes(-20), ImageName = "test2.jpg" },
                    new TimelineItemDTO { Timestamp = DateTime.Now.AddMinutes(-10), ImageName = "test3.jpg" },
                    new TimelineItemDTO { Timestamp = DateTime.Now, ImageName = "test4.jpg" }
                };
            }
        };

        ViewModel.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == nameof(ViewModel.ZoomFactor))
            {
                ImageScroller?.ChangeView(
                    ImageScroller.HorizontalOffset,
                    ImageScroller.VerticalOffset,
                    ViewModel.ZoomFactor
                );
            }
            else if (e.PropertyName == nameof(ViewModel.TargetRotationAngle))
            {
                AnimateRotation();
            }
        };
    }

    private void AnimateRotation()
    {
        if (ImgRotate == null)
            return;

        var animation = new DoubleAnimation
        {
            From = ViewModel.CurrentRotationAngle,
            To = ViewModel.TargetRotationAngle,
            Duration = new Duration(TimeSpan.FromMilliseconds(300)),
            EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseInOut }
        };

        Storyboard.SetTarget(animation, ImgRotate);
        Storyboard.SetTargetProperty(animation, "Angle");

        var storyboard = new Storyboard();
        storyboard.Children.Add(animation);
        storyboard.Begin();
    }
}