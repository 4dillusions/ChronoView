/*
4di .NET ChronoView application
Copyright (c) 2025 by 4D Illusions. All rights reserved.
Released under the terms of the GNU General Public License version 3 or later.
*/

using System;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
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
    private const double ThumbHeight = 60;
    private const double LabelHeight = 18;

    private Border? _selectedThumb;
    private bool _isSyncingScroll;

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
                if (_isSyncingScroll) return;
                _isSyncingScroll = true;
                try
                {
                    LabelScroller.Offset = new Vector(offset.X, 0);
                }
                finally
                {
                    _isSyncingScroll = false;
                }
            }));

        SizeChanged += (_, __) => RedrawTimeline();
    }

    private void TryWireVmFromDataContext()
    {
        if (DataContext is TimelineViewModel vm && !ReferenceEquals(ViewModel, vm))
            ViewModel = vm;
    }

    private void WireViewModel()
    {
        if (_wiredVm != null)
            _wiredVm.PropertyChanged -= VmOnPropertyChanged;

        _wiredVm = ViewModel;
        if (_wiredVm == null)
            return;

        DataContext = _wiredVm;
        _wiredVm.PropertyChanged += VmOnPropertyChanged;

        // DP -> VM sync
        _wiredVm.IsLocked = IsLocked;

        RedrawTimeline();
    }

    private TimelineViewModel? _wiredVm;

    private void VmOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(TimelineViewModel.RedrawTrigger) ||
            e.PropertyName == nameof(TimelineViewModel.TimelineWidth) ||
            e.PropertyName == nameof(TimelineViewModel.Items))
        {
            RedrawTimeline();
        }
        else if (e.PropertyName == nameof(TimelineViewModel.SelectedTimeLineItem))
        {
            UpdateSelectedVisual();
        }
    }

    private void RedrawTimeline()
    {
        if (ThumbPanel == null || LabelPanel == null)
            return;

        ThumbPanel.Children.Clear();
        LabelPanel.Children.Clear();
        _selectedThumb = null;

        var vm = ViewModel ?? DataContext as TimelineViewModel;
        if (vm?.Items == null || vm.Items.Count == 0)
            return;

        var marker = MarkerBrush ?? Brushes.DodgerBlue;
        var hover = HoverBrush ?? Brushes.Gray;
        var selected = SelectedBrush ?? Brushes.White;

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

            var img = new Image { Stretch = Stretch.UniformToFill };
            try
            {
                img.Source = new Bitmap(item.ImagePath);
            }
            catch
            {
                // ignore load errors
            }

            border.Child = img;

            border.PointerPressed += (_, __) =>
            {
                if (IsLocked) return;
                vm.SelectedTimeLineItem = item;
            };

            border.PointerEntered += (_, __) =>
            {
                if (IsLocked) return;
                if (!ReferenceEquals(border, _selectedThumb))
                    border.BorderBrush = hover;
            };

            border.PointerExited += (_, __) =>
            {
                if (IsLocked) return;
                if (!ReferenceEquals(border, _selectedThumb))
                    border.BorderBrush = marker;
            };

            ThumbPanel.Children.Add(border);

            var label = new TextBlock
            {
                Text = item.Timestamp.ToString(TimeFormat, CultureInfo.InvariantCulture),
                Height = LabelHeight,
                FontSize = 10,
                Foreground = marker,
                Opacity = 0.9,
                TextAlignment = TextAlignment.Center,
                Width = ThumbHeight
            };

            LabelPanel.Children.Add(label);

            if (vm.SelectedTimeLineItem != null && ReferenceEquals(item, vm.SelectedTimeLineItem))
                ApplySelectedVisual(border, label, marker, selected);
        }
    }

    private void UpdateSelectedVisual()
    {
        var vm = ViewModel ?? DataContext as TimelineViewModel;
        if (vm == null || ThumbPanel == null || LabelPanel == null)
            return;

        var marker = MarkerBrush ?? Brushes.DodgerBlue;
        var selected = SelectedBrush ?? Brushes.White;

        if (_selectedThumb != null)
        {
            _selectedThumb.BorderBrush = marker;
            _selectedThumb.BorderThickness = new Thickness(2);

            var idx = ThumbPanel.Children.IndexOf(_selectedThumb);
            if (idx >= 0 && idx < LabelPanel.Children.Count && LabelPanel.Children[idx] is TextBlock oldLbl)
            {
                oldLbl.FontWeight = FontWeight.Normal;
                oldLbl.Foreground = marker;
            }

            _selectedThumb = null;
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
        _selectedThumb = border;

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

        // Defer until layout is stable
        Dispatcher.UIThread.Post(() =>
        {
            try
            {
                var bounds = selected.Bounds;
                var point = selected.TranslatePoint(new Point(0, 0), ThumbPanel) ?? new Point(0, 0);

                var target = Math.Max(0, point.X - TimelineScroller.Viewport.Width / 2 + bounds.Width / 2);
                TimelineScroller.Offset = new Vector(target, 0);
            }
            catch
            {
                // ignore
            }
        }, DispatcherPriority.Background);
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
