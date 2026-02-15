/*
4di .NET ChronoView application
Copyright (c) 2025 by 4D Illusions. All rights reserved.
Released under the terms of the GNU General Public License version 3 or later.
*/

using App4di.Dotnet.ChronoView.Infrastructure.ViewModel;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.ComponentModel;

namespace App4di.Dotnet.ChronoView.WinUI.Helpers;

public static class HomePageHelper
{
    public static readonly DependencyProperty ViewModelProperty =
        DependencyProperty.RegisterAttached(
            "ViewModel",
            typeof(HomeViewModel),
            typeof(HomePageHelper),
            new PropertyMetadata(null, OnViewModelChanged));

    public static void SetViewModel(DependencyObject obj, HomeViewModel value) => obj.SetValue(ViewModelProperty, value);
    public static HomeViewModel GetViewModel(DependencyObject obj) => (HomeViewModel)obj.GetValue(ViewModelProperty);

    private static readonly DependencyProperty StateProperty =
        DependencyProperty.RegisterAttached("State", typeof(State), typeof(HomePageHelper), new PropertyMetadata(null));

    private static State GetState(DependencyObject obj)
    {
        var s = (State)obj.GetValue(StateProperty);
        if (s == null)
        {
            s = new State();
            obj.SetValue(StateProperty, s);
        }
        return s;
    }

    private static void EnsureSlideshowTimer(State s)
    {
        if (s.SlideshowTimer != null)
            return;

        s.SlideshowTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(2)
        };

        s.SlideshowTickHandler = (_, __) =>
        {
            if (s.VM?.TimelineItems == null || s.VM.TimelineItems.Count == 0)
                return;

            if (s.VM.SelectedIndex < 0)
                s.VM.SelectedIndex = 0;
            else
                s.VM.SelectedIndex = (s.VM.SelectedIndex + 1) % s.VM.TimelineItems.Count;
        };

        s.SlideshowTimer.Tick += s.SlideshowTickHandler;
    }

    private static void HandleAutoPlayChanged(State s)
    {
        EnsureSlideshowTimer(s);

        if (s.VM == null || s.SlideshowTimer == null)
            return;

        if (s.VM.IsAutoPlay)
        {
            if (s.VM.SelectedIndex < 0 && s.VM.TimelineItems?.Count > 0)
                s.VM.SelectedIndex = 0;

            s.SlideshowTimer.Start();
        }
        else
        {
            s.SlideshowTimer.Stop();
        }
    }

    private static void OnViewModelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not FrameworkElement root) 
            return;

        var s = GetState(root);

        if (s.SlideshowTimer != null)
        {
            s.SlideshowTimer.Stop();
            if (s.SlideshowTickHandler != null)
                s.SlideshowTimer.Tick -= s.SlideshowTickHandler;

            s.SlideshowTimer = null;
            s.SlideshowTickHandler = null;
        }

        if (e.OldValue is HomeViewModel oldVm && s.VmHandler != null)
            oldVm.PropertyChanged -= s.VmHandler;

        s.Root = root;
        s.ImageScroller = root.FindName("ImageScroller") as ScrollViewer;
        s.ImageView = root.FindName("ImageView") as Image;
        s.Rotate = root.FindName("ImgRotate") as RotateTransform;

        if (s.ImageScroller != null)
        {
            if (s.ScrollerSizeHandler != null) s.ImageScroller.SizeChanged -= s.ScrollerSizeHandler;
            s.ScrollerSizeHandler = (_, __) =>
            {
                UpdateViewportSizes(s);

                if (s.VM?.ShouldFitToViewport == true)
                    FitToViewport(s, disableAnimation: true);
            };
            s.ImageScroller.SizeChanged += s.ScrollerSizeHandler;
        }

        if (s.ImageView != null)
        {
            if (s.ImageOpenedHandler != null) s.ImageView.ImageOpened -= s.ImageOpenedHandler;
            s.ImageOpenedHandler = (_, __) =>
            {
                UpdateImageSizes(s);
                if (s.VM?.ShouldFitToViewport == true)
                    FitToViewport(s, disableAnimation: true);
            };
            s.ImageView.ImageOpened += s.ImageOpenedHandler;
        }

        s.VM = e.NewValue as HomeViewModel;
        if (s.VM == null) return;

        s.VmHandler = (_, ev) =>
        {
            if (ev.PropertyName == nameof(HomeViewModel.ZoomFactor))
            {
                ApplyZoom(s, disableAnimation: false);
            }
            else if (ev.PropertyName == nameof(HomeViewModel.TargetRotationAngle))
            {
                AnimateRotation(s);
            }
            else if (ev.PropertyName == nameof(HomeViewModel.SelectedImageItem))
            {
                s.VM.ShouldFitToViewport = true;
                //FitToViewport(s, disableAnimation: true);
            }
            else if (ev.PropertyName == nameof(HomeViewModel.ShouldFitToViewport))
            {
                if (s.VM.ShouldFitToViewport)
                    FitToViewport(s, disableAnimation: true);
            }
            else if (ev.PropertyName == nameof(HomeViewModel.IsAutoPlay))
            {
                HandleAutoPlayChanged(s);
            }
        };

        s.VM.PropertyChanged += s.VmHandler;

        EnsureSlideshowTimer(s);
        HandleAutoPlayChanged(s);

        UpdateViewportSizes(s);
        UpdateImageSizes(s);
        ApplyZoom(s, disableAnimation: true);

        if (s.VM.ShouldFitToViewport)
            FitToViewport(s, disableAnimation: true);
    }

    private static void UpdateViewportSizes(State s)
    {
        if (s.ImageScroller == null || s.VM == null) 
            return;

        s.VM.ViewportWidth = s.ImageScroller.ViewportWidth;
        s.VM.ViewportHeight = s.ImageScroller.ViewportHeight;
    }

    private static void UpdateImageSizes(State s)
    {
        if (s.ImageView == null || s.VM == null) 
            return;

        if (s.ImageView.Source is BitmapImage bmp && bmp.PixelWidth > 0 && bmp.PixelHeight > 0)
        {
            s.VM.ImageWidth = bmp.PixelWidth;
            s.VM.ImageHeight = bmp.PixelHeight;
        }
        else
        {
            s.VM.ImageWidth = s.ImageView.ActualWidth;
            s.VM.ImageHeight = s.ImageView.ActualHeight;
        }
    }

    private static void ApplyZoom(State s, bool disableAnimation)
    {
        if (s.ImageScroller == null || s.VM == null) 
            return;

        s.ImageScroller.ChangeView(
            s.ImageScroller.HorizontalOffset,
            s.ImageScroller.VerticalOffset,
            s.VM.ZoomFactor,
            disableAnimation);
    }

    private static void FitToViewport(State s, bool disableAnimation)
    {
        if (s.ImageScroller == null || s.VM == null)
            return;

        UpdateViewportSizes(s);
        UpdateImageSizes(s);

        if (s.VM.ZoomFactor <= 0)
            return;

        s.ImageScroller.ChangeView(0, 0, s.VM.ZoomFactor, disableAnimation);
        s.VM.ShouldFitToViewport = false;
    }

    private static void AnimateRotation(State s)
    {
        if (s.VM == null || s.Rotate == null) return;

        var anim = new DoubleAnimation
        {
            From = s.VM.CurrentRotationAngle,
            To = s.VM.TargetRotationAngle,
            Duration = new Duration(TimeSpan.FromMilliseconds(300)),
            EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseInOut }
        };

        Storyboard.SetTarget(anim, s.Rotate);
        Storyboard.SetTargetProperty(anim, "Angle");

        var sb = new Storyboard();
        sb.Children.Add(anim);
        sb.Completed += (_, __) => s.VM.CurrentRotationAngle = s.VM.TargetRotationAngle;
        sb.Begin();
    }

    private sealed class State
    {
        public FrameworkElement? Root;
        public HomeViewModel? VM;

        public ScrollViewer? ImageScroller;
        public Image? ImageView;
        public RotateTransform? Rotate;

        public PropertyChangedEventHandler? VmHandler;
        public SizeChangedEventHandler? ScrollerSizeHandler;
        public RoutedEventHandler? ImageOpenedHandler;

        public DispatcherTimer? SlideshowTimer;
        public EventHandler<object>? SlideshowTickHandler;
    }
}
