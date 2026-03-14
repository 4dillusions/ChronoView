/*
4di .NET ChronoView application
Copyright (c) 2025 by 4D Illusions. All rights reserved.
Released under the terms of the GNU General Public License version 3 or later.
*/

using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using App4di.Dotnet.ChronoView.AvaloniaUI.Helpers;
using App4di.Dotnet.ChronoView.Infrastructure.DTO;
using App4di.Dotnet.ChronoView.Infrastructure.ViewModel;

namespace App4di.Dotnet.ChronoView.AvaloniaUI.Controls;

public partial class TimelineControl : UserControl
{
    private const double ThumbHeight = 52;
    private const double LabelHeight = 14;
    private const int ThumbnailDecodeWidth = 220;

    private Border? selectedThumb;
    private bool isSyncingScroll;
    private NotifyCollectionChangedEventHandler? itemsChangedHandler;
    private DispatcherTimer? centerAnimationTimer;

    public static readonly StyledProperty<TimelineViewModel?> ViewModelProperty =
        AvaloniaProperty.Register<TimelineControl, TimelineViewModel?>(nameof(ViewModel));

    public static readonly StyledProperty<IBrush?> MarkerBrushProperty =
        AvaloniaProperty.Register<TimelineControl, IBrush?>(nameof(MarkerBrush));

    public static readonly StyledProperty<IBrush?> HoverBrushProperty =
        AvaloniaProperty.Register<TimelineControl, IBrush?>(nameof(HoverBrush));

    public static readonly StyledProperty<IBrush?> SelectedBrushProperty =
        AvaloniaProperty.Register<TimelineControl, IBrush?>(nameof(SelectedBrush));

    public static readonly StyledProperty<bool> IsLockedProperty =
        AvaloniaProperty.Register<TimelineControl, bool>(nameof(IsLocked));

    public static readonly StyledProperty<string> TimeFormatProperty =
        AvaloniaProperty.Register<TimelineControl, string>(nameof(TimeFormat), "yy.MM.dd HH:mm:ss");

    public TimelineViewModel? ViewModel
    {
        get => GetValue(ViewModelProperty);
        set => SetValue(ViewModelProperty, value);
    }

    public IBrush? MarkerBrush
    {
        get => GetValue(MarkerBrushProperty);
        set => SetValue(MarkerBrushProperty, value);
    }

    public IBrush? HoverBrush
    {
        get => GetValue(HoverBrushProperty);
        set => SetValue(HoverBrushProperty, value);
    }

    public IBrush? SelectedBrush
    {
        get => GetValue(SelectedBrushProperty);
        set => SetValue(SelectedBrushProperty, value);
    }

    public bool IsLocked
    {
        get => GetValue(IsLockedProperty);
        set => SetValue(IsLockedProperty, value);
    }

    public string TimeFormat
    {
        get => GetValue(TimeFormatProperty);
        set => SetValue(TimeFormatProperty, value);
    }

    public TimelineControl()
    {
        InitializeComponent();

        // Default brushes from app resources (matches WinUI defaults)
        MarkerBrush ??= this.FindResource("Timeline.MarkerBrush") as IBrush;
        HoverBrush ??= this.FindResource("Timeline.HoverBrush") as IBrush;
        SelectedBrush ??= this.FindResource("Timeline.SelectedBrush") as IBrush;

        this.GetObservable(DataContextProperty)
            .Subscribe(new ActionObserver<object?>(_ => TryWireVmFromDataContext()));

        this.GetObservable(ViewModelProperty)
            .Subscribe(new ActionObserver<TimelineViewModel?>(_ => WireViewModel()));

        this.GetObservable(IsLockedProperty)
            .Subscribe(new ActionObserver<bool>(v =>
            {
                if (ViewModel != null)
                    ViewModel.IsLocked = v;
            }));

        // Scroll sync: thumbs -> labels
        TimelineScroller
            .GetObservable(ScrollViewer.OffsetProperty)
            .Subscribe(new ActionObserver<Vector>(offset =>
            {
                if (isSyncingScroll) return;
                isSyncingScroll = true;
                try
                {
                    LabelScroller.Offset = new Vector(offset.X, 0);
                }
                finally
                {
                    isSyncingScroll = false;
                }
            }));

        SizeChanged += (_, __) => RedrawTimeline();
        AttachedToVisualTree += (_, __) => ScheduleRedraw();
    }

    private void TryWireVmFromDataContext()
    {
        if (DataContext is TimelineViewModel vm && !ReferenceEquals(ViewModel, vm))
            ViewModel = vm;
    }

    private void WireViewModel()
    {
        if (wiredVm != null)
        {
            wiredVm.PropertyChanged -= VmOnPropertyChanged;
            UnwireItems(wiredVm);
        }

        wiredVm = ViewModel;
        if (wiredVm == null)
            return;

        DataContext = wiredVm;
        wiredVm.PropertyChanged += VmOnPropertyChanged;

        // DP -> VM sync
        wiredVm.IsLocked = IsLocked;

        WireItems(wiredVm);
        ScheduleRedraw();
    }

    private TimelineViewModel? wiredVm;

    private void VmOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(TimelineViewModel.RedrawTrigger) ||
            e.PropertyName == nameof(TimelineViewModel.TimelineWidth) ||
            e.PropertyName == nameof(TimelineViewModel.Items))
        {
            if (e.PropertyName == nameof(TimelineViewModel.Items) && sender is TimelineViewModel vm)
                WireItems(vm);

            ScheduleRedraw();
        }
        else if (e.PropertyName == nameof(TimelineViewModel.SelectedTimeLineItem))
        {
            UpdateSelectedVisual();
        }
    }

    private void WireItems(TimelineViewModel vm)
    {
        UnwireItems(vm);

        itemsChangedHandler = (_, __) => ScheduleRedraw();
        vm.Items.CollectionChanged += itemsChangedHandler;
    }

    private void UnwireItems(TimelineViewModel vm)
    {
        if (itemsChangedHandler == null)
            return;

        vm.Items.CollectionChanged -= itemsChangedHandler;
        itemsChangedHandler = null;
    }

    private void ScheduleRedraw()
    {
        Dispatcher.UIThread.Post(RedrawTimeline, DispatcherPriority.Render);
    }

    public void Refresh()
    {
        ScheduleRedraw();
    }

    public void SetTimelineInteractionEnabled(bool isEnabled)
    {
        IsLocked = !isEnabled;
        TimelineContent.IsHitTestVisible = isEnabled;
        TimelineContent.Opacity = isEnabled ? 1.0 : 0.65;
    }

    private void RedrawTimeline()
    {
        if (ThumbPanel == null || LabelPanel == null)
            return;

        ThumbPanel.Children.Clear();
        LabelPanel.Children.Clear();
        selectedThumb = null;

        var vm = ViewModel ?? DataContext as TimelineViewModel;
        if (vm?.Items == null || vm.Items.Count == 0)
            return;

        var marker = MarkerBrush ?? Brushes.DodgerBlue;
        var hover = HoverBrush ?? Brushes.Gray;
        var selected = SelectedBrush ?? Brushes.White;
        Border? selectedBorderToCenter = null;

        var sorted = vm.Items.OrderBy(i => i.Timestamp).ToList();

        foreach (var item in sorted)
        {
            var border = new Border
            {
                Width = ThumbHeight,
                Height = ThumbHeight,
                BorderThickness = new Thickness(2),
                BorderBrush = marker,
                Margin = new Thickness(0),
                Tag = item
            };

            var img = new Image { Stretch = Stretch.UniformToFill, Tag = item };
            _ = LoadThumbnailAsync(img, item);

            border.Child = img;

            border.PointerPressed += (_, __) =>
            {
                if (IsLocked) return;
                vm.SelectedTimeLineItem = item;
            };

            border.PointerEntered += (_, __) =>
            {
                if (IsLocked) return;
                if (!ReferenceEquals(border, selectedThumb))
                    border.BorderBrush = hover;
            };

            border.PointerExited += (_, __) =>
            {
                if (IsLocked) return;
                if (!ReferenceEquals(border, selectedThumb))
                    border.BorderBrush = marker;
            };

            ThumbPanel.Children.Add(border);

            var label = new TextBlock
            {
                Text = item.Timestamp.ToString(TimeFormat, CultureInfo.InvariantCulture),
                Height = LabelHeight,
                FontSize = 9,
                Foreground = marker,
                Opacity = 0.9,
                TextAlignment = TextAlignment.Center,
                Width = ThumbHeight
            };

            LabelPanel.Children.Add(label);

            if (vm.SelectedTimeLineItem != null && ReferenceEquals(item, vm.SelectedTimeLineItem))
            {
                ApplySelectedVisual(border, label, marker, selected);
                selectedBorderToCenter = border;
            }
        }

        ThumbPanel.InvalidateMeasure();
        ThumbPanel.InvalidateArrange();
        LabelPanel.InvalidateMeasure();
        LabelPanel.InvalidateArrange();
        TimelineScroller.InvalidateMeasure();
        TimelineScroller.InvalidateArrange();
        LabelScroller.InvalidateMeasure();
        LabelScroller.InvalidateArrange();

        if (selectedBorderToCenter != null)
            CenterSelectedIntoView(selectedBorderToCenter);
    }

    private async Task LoadThumbnailAsync(Image image, TimelineItemDTO item)
    {
        var bitmap = await BitmapCache.GetThumbnailAsync(item.ImagePath, ThumbnailDecodeWidth);
        if (!ReferenceEquals(image.Tag, item))
            return;

        image.Source = bitmap;
    }

    private void UpdateSelectedVisual()
    {
        var vm = ViewModel ?? DataContext as TimelineViewModel;
        if (vm == null || ThumbPanel == null || LabelPanel == null)
            return;

        var marker = MarkerBrush ?? Brushes.DodgerBlue;
        var selected = SelectedBrush ?? Brushes.White;

        if (selectedThumb != null)
        {
            selectedThumb.BorderBrush = marker;
            selectedThumb.BorderThickness = new Thickness(2);

            var idx = ThumbPanel.Children.IndexOf(selectedThumb);
            if (idx >= 0 && idx < LabelPanel.Children.Count && LabelPanel.Children[idx] is TextBlock oldLbl)
            {
                oldLbl.FontWeight = FontWeight.Normal;
                oldLbl.Foreground = marker;
            }

            selectedThumb = null;
        }

        if (vm.SelectedTimeLineItem == null)
            return;

        for (var i = 0; i < ThumbPanel.Children.Count; i++)
        {
            if (ThumbPanel.Children[i] is Border b && ReferenceEquals(b.Tag, vm.SelectedTimeLineItem))
            {
                var lbl = (i < LabelPanel.Children.Count) ? LabelPanel.Children[i] as TextBlock : null;
                ApplySelectedVisual(b, lbl, marker, selected);
                CenterSelectedIntoView(b);
                break;
            }
        }
    }

    private void ApplySelectedVisual(Border border, TextBlock? label, IBrush marker, IBrush selected)
    {
        border.BorderBrush = selected;
        border.BorderThickness = new Thickness(4);
        selectedThumb = border;

        if (label != null)
        {
            label.FontWeight = FontWeight.SemiBold;
            label.Foreground = selected;
        }
    }

    private void CenterSelectedIntoView(Border selected)
    {
        if (TimelineScroller == null || ThumbPanel == null)
            return;

        Dispatcher.UIThread.Post(() =>
        {
            try
            {
                var bounds = selected.Bounds;
                var point = selected.TranslatePoint(new Point(0, 0), ThumbPanel) ?? new Point(0, 0);
                var viewportWidth = TimelineScroller.Viewport.Width > 0
                    ? TimelineScroller.Viewport.Width
                    : TimelineScroller.Bounds.Width;
                var contentWidth = Math.Max(ThumbPanel.Bounds.Width, TimelineScroller.Extent.Width);
                var maxOffset = Math.Max(0, contentWidth - viewportWidth);
                var target = point.X - viewportWidth / 2 + bounds.Width / 2;
                target = Math.Clamp(target, 0, maxOffset);

                AnimateTimelineOffset(target);
            }
            catch
            {
                // ignore
            }
        }, DispatcherPriority.Render);
    }

    private void AnimateTimelineOffset(double targetX)
    {
        if (TimelineScroller == null)
            return;

        centerAnimationTimer?.Stop();

        var startX = TimelineScroller.Offset.X;
        if (Math.Abs(startX - targetX) < 0.5)
        {
            TimelineScroller.Offset = new Vector(targetX, 0);
            return;
        }

        var startedAt = DateTimeOffset.UtcNow;
        var duration = TimeSpan.FromMilliseconds(220);

        centerAnimationTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(16) };
        centerAnimationTimer.Tick += (_, __) =>
        {
            if (TimelineScroller == null || centerAnimationTimer == null)
                return;

            var progress = (DateTimeOffset.UtcNow - startedAt).TotalMilliseconds / duration.TotalMilliseconds;
            if (progress >= 1.0)
            {
                TimelineScroller.Offset = new Vector(targetX, 0);
                centerAnimationTimer.Stop();
                centerAnimationTimer = null;
                return;
            }

            var eased = progress < 0.5
                ? 2 * progress * progress
                : 1 - Math.Pow(-2 * progress + 2, 2) / 2;
            var currentX = startX + ((targetX - startX) * eased);
            TimelineScroller.Offset = new Vector(currentX, 0);
        };

        centerAnimationTimer.Start();
    }

    // Backward-compat convenience (used by WinUI)
    public System.Collections.ObjectModel.ObservableCollection<TimelineItemDTO> Items
    {
        get => (ViewModel?.Items) ?? new System.Collections.ObjectModel.ObservableCollection<TimelineItemDTO>();
        set
        {
            if (ViewModel != null)
                ViewModel.Items = value;
        }
    }

    public TimelineItemDTO? SelectedTimeLineItem
    {
        get => ViewModel?.SelectedTimeLineItem;
        set
        {
            if (ViewModel != null)
                ViewModel.SelectedTimeLineItem = value;
        }
    }
}
