/*
4di .NET ChronoView application
Copyright (c) 2025 by 4D Illusions. All rights reserved.
Released under the terms of the GNU General Public License version 3 or later.
*/

using App4di.Dotnet.ChronoView.Infrastructure.DTO;
using App4di.Dotnet.ChronoView.Infrastructure.ViewModel;
using App4di.Dotnet.ChronoView.WinUI.Service;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using System;
using System.ComponentModel;

namespace App4di.Dotnet.ChronoView.WinUI.View;

public sealed partial class HomePage : Page
{
    public HomeViewModel ViewModel { get; }
    private readonly DispatcherTimer slideshowTimer;

    public HomePage()
    {
        InitializeComponent();
        ViewModel = new HomeViewModel(new FolderPickerService(), new Infrastructure.Service.FileService());
        DataContext = ViewModel;

        Loaded += OnPageLoaded;
        ViewModel.PropertyChanged += OnViewModelPropertyChanged;

        slideshowTimer = new DispatcherTimer();
        slideshowTimer.Interval = TimeSpan.FromSeconds(2);
        slideshowTimer.Tick += SlideshowTimer_Tick;
    }

    private void OnPageLoaded(object sender, RoutedEventArgs e)
    {
        if (Timeline?.ViewModel != null)
        {
            SyncTimelineItems();
            Timeline.ViewModel.SelectedItemChanged += OnTimelineSelectionChanged;
        }
    }

    private void SyncTimelineItems()
    {
        if (Timeline?.ViewModel != null && ViewModel?.TimelineItems != null)
        {
            Timeline.ViewModel.Items = ViewModel.TimelineItems;
            if (ViewModel.SelectedImageItem != null)
                Timeline.ViewModel.SelectedTimeLineItem = ViewModel.SelectedImageItem;
        }
    }

    private void OnTimelineSelectionChanged(object? sender, TimelineItemDTO? selectedItem)
    {
        if (selectedItem != null)
        {
            ViewModel.SelectedImageItem = selectedItem;
        }
    }

    private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(ViewModel.ZoomFactor))
        {
            ImageScroller?.ChangeView(ImageScroller.HorizontalOffset, ImageScroller.VerticalOffset, ViewModel.ZoomFactor, disableAnimation: false);
        }
        else if (e.PropertyName == nameof(ViewModel.TargetRotationAngle))
        {
            AnimateRotation();
        }
        else if (e.PropertyName == nameof(ViewModel.TimelineItems))
        {
            SyncTimelineItems();
        }
        else if (e.PropertyName == nameof(ViewModel.IsAutoPlay))
        {
            HandleAutoPlayChanged();
        }
        else if (e.PropertyName == nameof(ViewModel.SelectedImageItem))
        {
            HandleSelectedImageChanged();
        }
        else if (e.PropertyName == nameof(ViewModel.ShouldFitToViewport))
        {
            if (ViewModel.ShouldFitToViewport && ImageScroller != null)
            {
                ImageScroller.ChangeView(0, 0, ViewModel.ZoomFactor, disableAnimation: false);
                ViewModel.ShouldFitToViewport = false;
            }
        }
    }

    private void HandleAutoPlayChanged()
    {
        if (ViewModel.IsAutoPlay)
        {
            if (ViewModel.SelectedIndex < 0 && ViewModel.TimelineItems?.Count > 0)
                ViewModel.SelectedIndex = 0;

            slideshowTimer.Start();
        }
        else
        {
            slideshowTimer.Stop();
        }
    }

    private void HandleSelectedImageChanged()
    {
        if (Timeline?.ViewModel != null)
        {
            Timeline.ViewModel.SelectedTimeLineItem = ViewModel.SelectedImageItem;

            if (ViewModel.SelectedImageItem is not null)
            {
                var pos = Timeline.ViewModel.CalculateMarkerPosition(ViewModel.SelectedImageItem.Timestamp);
                if (Timeline.FindName("TimelineScroller") is ScrollViewer sc)
                {
                    var target = Math.Max(0, pos - sc.ViewportWidth / 2.0);
                    sc.ChangeView(target, null, null, disableAnimation: false);
                }
            }
        }
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

    private void SlideshowTimer_Tick(object? sender, object e)
    {
        if (ViewModel.TimelineItems == null || ViewModel.TimelineItems.Count == 0)
            return;

        if (ViewModel.SelectedIndex < 0)
            ViewModel.SelectedIndex = 0;
        else
            ViewModel.SelectedIndex = (ViewModel.SelectedIndex + 1) % ViewModel.TimelineItems.Count;
    }

    protected override void OnNavigatedFrom(Microsoft.UI.Xaml.Navigation.NavigationEventArgs e)
    {
        slideshowTimer.Stop();
        base.OnNavigatedFrom(e);
    }

    private void ImageScroller_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        if (ImageScroller != null)
        {
            ViewModel.ViewportWidth = ImageScroller.ViewportWidth;
            ViewModel.ViewportHeight = ImageScroller.ViewportHeight;
        }
    }

    private void ImageView_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        if (ImageView?.Source is Microsoft.UI.Xaml.Media.Imaging.BitmapImage bmp)
        {
            ViewModel.ImageWidth = bmp.PixelWidth > 0 ? bmp.PixelWidth : ImageView.ActualWidth;
            ViewModel.ImageHeight = bmp.PixelHeight > 0 ? bmp.PixelHeight : ImageView.ActualHeight;
        }
    }
}