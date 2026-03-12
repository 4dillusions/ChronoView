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

    private DispatcherTimer? _slideshowTimer;
    private EventHandler? _slideshowTick;

    private IDisposable? _viewportSub;
    private IDisposable? _extentSub;

    private bool _isAnimatingRotation;

    private ScaleTransform? _imgScale;
    private RotateTransform? _imgRotate;
    private PropertyChangedEventHandler? _timelineViewModelHandler;

    public HomeView(HomeViewModel viewModel)
    {
        ViewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
        DataContext = ViewModel;

        InitializeComponent();

        CacheTransforms();

        // Viewport changes (resize, layout)
        _viewportSub = ImageScroller
            .GetObservable(ScrollViewer.ViewportProperty)
            .Subscribe(new ActionObserver<Size>(_ =>
            {
                UpdateViewportSizes();
                if (ViewModel.ShouldFitToViewport)
                    FitToViewport(disableOffsetAnimation: true);
            }));

        // Extent changes are a good proxy for content size updates
        _extentSub = ImageScroller
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
        _timelineViewModelHandler = (_, e) =>
        {
            if (e.PropertyName == nameof(TimelineViewModel.RedrawTrigger) ||
                e.PropertyName == nameof(TimelineViewModel.Items) ||
                e.PropertyName == nameof(TimelineViewModel.SelectedTimeLineItem))
            {
                Timeline.Refresh();
            }
        };
        ViewModel.TimelineViewModel.PropertyChanged += _timelineViewModelHandler;

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
        if (_timelineViewModelHandler != null)
            ViewModel.TimelineViewModel.PropertyChanged -= _timelineViewModelHandler;

        if (_slideshowTimer != null && _slideshowTick != null)
            _slideshowTimer.Tick -= _slideshowTick;

        _slideshowTimer?.Stop();
        _slideshowTimer = null;
        _slideshowTick = null;

        _viewportSub?.Dispose();
        _extentSub?.Dispose();
        _viewportSub = null;
        _extentSub = null;
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
        if (_slideshowTimer != null)
            return;

        _slideshowTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(2)
        };

        _slideshowTick = (_, __) =>
        {
            if (ViewModel.TimelineItems == null || ViewModel.TimelineItems.Count == 0)
                return;

            if (ViewModel.SelectedIndex < 0)
                ViewModel.SelectedIndex = 0;
            else
                ViewModel.SelectedIndex = (ViewModel.SelectedIndex + 1) % ViewModel.TimelineItems.Count;
        };

        _slideshowTimer.Tick += _slideshowTick;
    }

    private void HandleAutoPlayChanged()
    {
        EnsureSlideshowTimer();

        if (_slideshowTimer == null)
            return;

        if (ViewModel.IsAutoPlay)
        {
            if (ViewModel.SelectedIndex < 0 && ViewModel.TimelineItems?.Count > 0)
                ViewModel.SelectedIndex = 0;

            _slideshowTimer.Start();
        }
        else
        {
            _slideshowTimer.Stop();
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
        if (_imgScale == null)
            return;

        var z = Math.Max(0.01, ViewModel.ZoomFactor);
        _imgScale.ScaleX = z;
        _imgScale.ScaleY = z;
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
        if (_imgRotate == null || _isAnimatingRotation)
            return;

        var from = ViewModel.CurrentRotationAngle;
        var to = ViewModel.TargetRotationAngle;

        _isAnimatingRotation = true;

        var start = DateTimeOffset.UtcNow;
        var duration = TimeSpan.FromMilliseconds(300);

        var timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(16) };
        timer.Tick += (_, __) =>
        {
            var t = (DateTimeOffset.UtcNow - start).TotalMilliseconds / duration.TotalMilliseconds;
            if (t >= 1.0)
            {
                _imgRotate.Angle = to;
                timer.Stop();
                _isAnimatingRotation = false;

                // keep VM in sync (matches WinUI Completed handler)
                ViewModel.CurrentRotationAngle = to;
                return;
            }

            // ease-in-out (quadratic-ish)
            var eased = t < 0.5 ? 2 * t * t : 1 - Math.Pow(-2 * t + 2, 2) / 2;
            _imgRotate.Angle = from + (to - from) * eased;
        };

        timer.Start();
    }

    private void CacheTransforms()
    {
        try
        {
            if (ImageView.RenderTransform is TransformGroup tg)
            {
                _imgScale = tg.Children.OfType<ScaleTransform>().FirstOrDefault();
                _imgRotate = tg.Children.OfType<RotateTransform>().FirstOrDefault();
            }
        }
        catch
        {
            _imgScale = null;
            _imgRotate = null;
        }
    }
}
