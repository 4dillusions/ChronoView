/*
4di .NET ChronoView application
Copyright (c) 2025 by 4D Illusions. All rights reserved.
Released under the terms of the GNU General Public License version 3 or later.
*/

using App4di.Dotnet.ChronoView.Infrastructure.DTO;
using App4di.Dotnet.ChronoView.Infrastructure.ViewModel;
using Microsoft.UI;
using Microsoft.UI.Text;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace App4di.Dotnet.ChronoView.WinUI.Control;

public sealed partial class TimelineControl : UserControl
{
    #region Fields
    private Border? selectedThumb;
    private const double ThumbHeight = 60;
    private const double LabelHeight = 18;
    #endregion

    #region Dependency Properties
    public static readonly DependencyProperty ViewModelProperty =
        DependencyProperty.Register(nameof(ViewModel), typeof(TimelineViewModel), typeof(TimelineControl),
            new PropertyMetadata(null, OnViewModelChanged));

    public static readonly DependencyProperty MarkerBrushProperty =
        DependencyProperty.Register(nameof(MarkerBrush), typeof(Brush), typeof(TimelineControl),
            new PropertyMetadata(new SolidColorBrush(Colors.DodgerBlue)));

    public static readonly DependencyProperty HoverBrushProperty =
        DependencyProperty.Register(nameof(HoverBrush), typeof(Brush), typeof(TimelineControl),
            new PropertyMetadata(new SolidColorBrush(Colors.Black)));

    public static readonly DependencyProperty SelectedBrushProperty =
        DependencyProperty.Register(nameof(SelectedBrush), typeof(Brush), typeof(TimelineControl),
            new PropertyMetadata(new SolidColorBrush(Colors.White)));

    public static readonly DependencyProperty IsLockedProperty =
        DependencyProperty.Register(nameof(IsLocked), typeof(bool), typeof(TimelineControl),
            new PropertyMetadata(false, OnIsLockedChanged));

    public static readonly DependencyProperty TimeFormatProperty =
        DependencyProperty.Register(nameof(TimeFormat), typeof(string), typeof(TimelineControl),
            new PropertyMetadata("yy.MM.dd HH:mm:ss"));

    private static void OnIsLockedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not TimelineControl control)
            return;

        if (control.ViewModel is not null)
            control.ViewModel.IsLocked = (bool)e.NewValue;
    }

    public TimelineViewModel ViewModel
    {
        get => (TimelineViewModel)GetValue(ViewModelProperty);
        set => SetValue(ViewModelProperty, value);
    }

    public Brush MarkerBrush
    {
        get => (Brush)GetValue(MarkerBrushProperty);
        set => SetValue(MarkerBrushProperty, value);
    }

    public Brush HoverBrush
    {
        get => (Brush)GetValue(HoverBrushProperty);
        set => SetValue(HoverBrushProperty, value);
    }

    public Brush SelectedBrush
    {
        get => (Brush)GetValue(SelectedBrushProperty);
        set => SetValue(SelectedBrushProperty, value);
    }

    public bool IsLocked
    {
        get => (bool)GetValue(IsLockedProperty);
        set => SetValue(IsLockedProperty, value);
    }

    public string TimeFormat
    {
        get => (string)GetValue(TimeFormatProperty);
        set => SetValue(TimeFormatProperty, value);
    }
    #endregion

    #region Constructor
    public TimelineControl()
    {
        InitializeComponent();

        SizeChanged += (_, __) => RedrawTimeline();

        TimelineScroller.ViewChanged += (_, __) =>
        {
            LabelScroller?.ChangeView(TimelineScroller.HorizontalOffset, null, null, disableAnimation: true);
        };
    }

    // backward compatibility
    public void SetViewModel(TimelineViewModel vm)
    {
        ViewModel = vm;
        DataContext = vm;
    }
    #endregion

    #region ViewModel wiring
    private static void OnViewModelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not TimelineControl control)
            return;

        if (e.OldValue is TimelineViewModel oldVm)
            oldVm.PropertyChanged -= control.ViewModel_PropertyChanged;

        if (e.NewValue is TimelineViewModel newVm)
        {
            control.DataContext = newVm;
            newVm.PropertyChanged += control.ViewModel_PropertyChanged;

            // DP -> VM sync
            newVm.IsLocked = control.IsLocked;

            control.RedrawTimeline();
        }
    }

    private void ViewModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(TimelineViewModel.RedrawTrigger) ||
            e.PropertyName == nameof(TimelineViewModel.TimelineWidth))
        {
            RedrawTimeline();
        }
        else if (e.PropertyName == nameof(TimelineViewModel.SelectedTimeLineItem))
        {
            UpdateSelectedVisual();
        }
    }
    #endregion

    #region Thumbnail timeline
    private void RedrawTimeline()
    {
        if (ThumbPanel == null || LabelPanel == null)
            return;

        ThumbPanel.Children.Clear();
        LabelPanel.Children.Clear();
        selectedThumb = null;

        if (ViewModel?.Items == null || ViewModel.Items.Count == 0)
            return;

        var thumbSize = ThumbHeight;
        var sorted = ViewModel.Items.OrderBy(i => i.Timestamp).ToList();

        foreach (var item in sorted)
        {
            // --- THUMB ---
            var border = new Border
            {
                Width = thumbSize,
                Height = thumbSize,
                BorderThickness = new Thickness(2),
                BorderBrush = MarkerBrush,
                Margin = new Thickness(0),
                Tag = item
            };

            var img = new Image { Stretch = Stretch.UniformToFill };

            try
            {
                var bmp = new BitmapImage
                {
                    DecodePixelWidth = (int)Math.Min(thumbSize, 220),
                    UriSource = new Uri(item.ImagePath)
                };
                img.Source = bmp;
            }
            catch
            {
                
            }

            border.Child = img;

            border.Tapped += (_, __) =>
            {
                if (IsLocked) return;
                ViewModel.SelectedTimeLineItem = item;
            };

            border.PointerEntered += (_, __) =>
            {
                if (IsLocked) return;
                if (!ReferenceEquals(border, selectedThumb))
                    border.BorderBrush = HoverBrush;
            };

            border.PointerExited += (_, __) =>
            {
                if (IsLocked) return;
                if (!ReferenceEquals(border, selectedThumb))
                    border.BorderBrush = MarkerBrush;
            };

            ThumbPanel.Children.Add(border);

            // --- LABEL ---
            var label = new TextBlock
            {
                Text = item.Timestamp.ToString(TimeFormat),
                Height = LabelHeight,
                FontSize = 10,
                Foreground = MarkerBrush,
                Opacity = 0.9,
                TextAlignment = TextAlignment.Center,
                Width = thumbSize
            };

            LabelPanel.Children.Add(label);

            // select
            if (ViewModel.SelectedTimeLineItem != null && ReferenceEquals(item, ViewModel.SelectedTimeLineItem))
            {
                ApplySelectedVisual(border, label);
            }
        }
    }

    private void UpdateSelectedVisual()
    {
        if (ThumbPanel == null || LabelPanel == null)
            return;

        if (selectedThumb != null)
        {
            selectedThumb.BorderBrush = MarkerBrush;
            selectedThumb.BorderThickness = new Thickness(2);

            var idx = ThumbPanel.Children.IndexOf(selectedThumb);
            if (idx >= 0 && idx < LabelPanel.Children.Count && LabelPanel.Children[idx] is TextBlock oldLbl)
            {
                oldLbl.FontWeight = FontWeights.Normal;
                oldLbl.Foreground = MarkerBrush;
            }

            selectedThumb = null;
        }

        if (ViewModel?.SelectedTimeLineItem == null)
            return;

        for (int i = 0; i < ThumbPanel.Children.Count; i++)
        {
            if (ThumbPanel.Children[i] is Border b && ReferenceEquals(b.Tag, ViewModel.SelectedTimeLineItem))
            {
                var lbl = (i < LabelPanel.Children.Count) ? LabelPanel.Children[i] as TextBlock : null;
                ApplySelectedVisual(b, lbl);
                CenterSelectedIntoView(b);
                break;
            }
        }
    }

    private void ApplySelectedVisual(Border border, TextBlock? label)
    {
        border.BorderBrush = SelectedBrush;
        border.BorderThickness = new Thickness(4);
        selectedThumb = border;

        if (label != null)
        {
            label.FontWeight = FontWeights.SemiBold;
            label.Foreground = SelectedBrush;
        }
    }

    private void CenterSelectedIntoView(Border selected)
    {
        if (TimelineScroller == null || ThumbPanel == null)
            return;

        var t = selected.TransformToVisual(ThumbPanel);
        var p = t.TransformPoint(new Windows.Foundation.Point(0, 0));

        var target = Math.Max(0, p.X - TimelineScroller.ViewportWidth / 2 + selected.ActualWidth / 2);
        TimelineScroller.ChangeView(target, null, null, disableAnimation: false);
    }
    #endregion

    #region Public Methods (for backward compatibility)
    public ObservableCollection<TimelineItemDTO> Items
    {
        get => ViewModel?.Items ?? new ObservableCollection<TimelineItemDTO>();
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

    public void ZoomIn() => ViewModel?.ZoomInCommand?.Execute(null);
    public void ZoomOut() => ViewModel?.ZoomOutCommand?.Execute(null);
    public void ResetZoom() => ViewModel?.ResetZoomCommand?.Execute(null);
    #endregion
}
