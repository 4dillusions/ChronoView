/*
4di .NET ChronoView application
Copyright (c) 2025 by 4D Illusions. All rights reserved.
Released under the terms of the GNU General Public License version 3 or later.
*/

using System;
using System.ComponentModel;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using App4di.Dotnet.ChronoView.AvaloniaUI.Helpers;
using App4di.Dotnet.ChronoView.Infrastructure.ViewModel;

namespace App4di.Dotnet.ChronoView.AvaloniaUI.View;

public partial class HomeView : UserControl
{
    public HomeViewModel ViewModel { get; }

    private DispatcherTimer? slideshowTimer;
    private EventHandler? slideshowTick;

    private IDisposable? viewportSub;
    private IDisposable? extentSub;

    private bool isAnimatingRotation;

    private ScaleTransform? imgScale;
    private RotateTransform? imgRotate;
    private PropertyChangedEventHandler? timelineViewModelHandler;

    public HomeView(HomeViewModel viewModel)
    {
        ViewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
        DataContext = ViewModel;

        InitializeComponent();

        CacheTransforms();

        // Viewport changes (resize, layout)
        viewportSub = ImageScroller
            .GetObservable(ScrollViewer.ViewportProperty)
            .Subscribe(new ActionObserver<Size>(_ =>
            {
                UpdateViewportSizes();
                if (ViewModel.ShouldFitToViewport)
                    FitToViewport(disableOffsetAnimation: true);
            }));

        // Extent changes are a good proxy for content size updates
        extentSub = ImageScroller
            .GetObservable(ScrollViewer.ExtentProperty)
            .Subscribe(new ActionObserver<Size>(_ =>
            {
                UpdateViewportSizes();
            }));

        ImageView.AttachedToVisualTree += (_, __) =>
        {
            UpdateViewportSizes();
            UpdateImageSizes();
            ApplyZoom();
            if (ViewModel.ShouldFitToViewport)
                FitToViewport(disableOffsetAnimation: true);
        };

        ImageView.PropertyChanged += (_, e) =>
        {
            if (e.Property == Image.SourceProperty)
            {
                UpdateImageSizes();
                // VM expects a fit after image change
                if (ViewModel.ShouldFitToViewport)
                    FitToViewport(disableOffsetAnimation: true);
            }
        };

        ViewModel.PropertyChanged += ViewModelOnPropertyChanged;
        timelineViewModelHandler = (_, e) =>
        {
            if (e.PropertyName == nameof(TimelineViewModel.RedrawTrigger) ||
                e.PropertyName == nameof(TimelineViewModel.Items) ||
                e.PropertyName == nameof(TimelineViewModel.SelectedTimeLineItem))
            {
                Timeline.Refresh();
            }
        };
        ViewModel.TimelineViewModel.PropertyChanged += timelineViewModelHandler;

        EnsureSlideshowTimer();
        HandleAutoPlayChanged();

        UpdateViewportSizes();
        UpdateImageSizes();
        ApplyZoom();
        if (ViewModel.ShouldFitToViewport)
            FitToViewport(disableOffsetAnimation: true);
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);

        ViewModel.PropertyChanged -= ViewModelOnPropertyChanged;
        if (timelineViewModelHandler != null)
            ViewModel.TimelineViewModel.PropertyChanged -= timelineViewModelHandler;

        if (slideshowTimer != null && slideshowTick != null)
            slideshowTimer.Tick -= slideshowTick;

        slideshowTimer?.Stop();
        slideshowTimer = null;
        slideshowTick = null;

        viewportSub?.Dispose();
        extentSub?.Dispose();
        viewportSub = null;
        extentSub = null;
    }

    private void ViewModelOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(HomeViewModel.ZoomFactor))
        {
            ApplyZoom();
        }
        else if (e.PropertyName == nameof(HomeViewModel.TargetRotationAngle))
        {
            AnimateRotation();
        }
        else if (e.PropertyName == nameof(HomeViewModel.SelectedImageItem))
        {
            // Same behavior as WinUI helper: on image switch request a fit.
            ViewModel.ShouldFitToViewport = true;
        }
        else if (e.PropertyName == nameof(HomeViewModel.ShouldFitToViewport))
        {
            if (ViewModel.ShouldFitToViewport)
                FitToViewport(disableOffsetAnimation: true);
        }
        else if (e.PropertyName == nameof(HomeViewModel.IsAutoPlay))
        {
            HandleAutoPlayChanged();
        }
    }

    private void EnsureSlideshowTimer()
    {
        if (slideshowTimer != null)
            return;

        slideshowTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(2)
        };

        slideshowTick = (_, __) =>
        {
            if (ViewModel.TimelineItems == null || ViewModel.TimelineItems.Count == 0)
                return;

            if (ViewModel.SelectedIndex < 0)
                ViewModel.SelectedIndex = 0;
            else
                ViewModel.SelectedIndex = (ViewModel.SelectedIndex + 1) % ViewModel.TimelineItems.Count;
        };

        slideshowTimer.Tick += slideshowTick;
    }

    private void HandleAutoPlayChanged()
    {
        EnsureSlideshowTimer();

        if (slideshowTimer == null)
            return;

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

    private void UpdateViewportSizes()
    {
        try
        {
            ViewModel.ViewportWidth = ImageScroller.Viewport.Width;
            ViewModel.ViewportHeight = ImageScroller.Viewport.Height;
        }
        catch
        {
            // ignore transient layout states
        }
    }

    private void UpdateImageSizes()
    {
        try
        {
            if (ImageView.Source is Bitmap bmp)
            {
                ViewModel.ImageWidth = bmp.Size.Width;
                ViewModel.ImageHeight = bmp.Size.Height;
            }
            else
            {
                ViewModel.ImageWidth = ImageView.Bounds.Width;
                ViewModel.ImageHeight = ImageView.Bounds.Height;
            }
        }
        catch
        {
            // ignore
        }
    }

    private void ApplyZoom()
    {
        if (imgScale == null)
            return;

        var z = Math.Max(0.01, ViewModel.ZoomFactor);
        imgScale.ScaleX = z;
        imgScale.ScaleY = z;
    }

    private void FitToViewport(bool disableOffsetAnimation)
    {
        UpdateViewportSizes();
        UpdateImageSizes();

        ApplyZoom();

        // Reset pan to top-left (closest to WinUI ChangeView(0,0,zoom))
        try
        {
            ImageScroller.Offset = new Avalonia.Vector(0, 0);
        }
        catch
        {
            // ignore
        }

        ViewModel.ShouldFitToViewport = false;
    }

    private void AnimateRotation()
    {
        if (imgRotate == null || isAnimatingRotation)
            return;

        var from = ViewModel.CurrentRotationAngle;
        var to = ViewModel.TargetRotationAngle;

        isAnimatingRotation = true;

        var start = DateTimeOffset.UtcNow;
        var duration = TimeSpan.FromMilliseconds(300);

        var timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(16) };
        timer.Tick += (_, __) =>
        {
            var t = (DateTimeOffset.UtcNow - start).TotalMilliseconds / duration.TotalMilliseconds;
            if (t >= 1.0)
            {
                imgRotate.Angle = to;
                timer.Stop();
                isAnimatingRotation = false;

                // keep VM in sync (matches WinUI Completed handler)
                ViewModel.CurrentRotationAngle = to;
                return;
            }

            // ease-in-out (quadratic-ish)
            var eased = t < 0.5 ? 2 * t * t : 1 - Math.Pow(-2 * t + 2, 2) / 2;
            imgRotate.Angle = from + (to - from) * eased;
        };

        timer.Start();
    }

    private void CacheTransforms()
    {
        try
        {
            if (ImageView.RenderTransform is TransformGroup tg)
            {
                imgScale = tg.Children.OfType<ScaleTransform>().FirstOrDefault();
                imgRotate = tg.Children.OfType<RotateTransform>().FirstOrDefault();
            }
        }
        catch
        {
            imgScale = null;
            imgRotate = null;
        }
    }
}
