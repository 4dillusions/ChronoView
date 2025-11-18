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
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI.Xaml.Shapes;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace App4di.Dotnet.ChronoView.WinUI.Control;

public sealed partial class TimelineControl : UserControl
{
    #region Fields
    private Canvas? markerCanvas;
    private Canvas? timelineCanvas;
    private Grid? contentGrid;
    private Line? selectedLine;
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
            new PropertyMetadata(false));

    public static readonly DependencyProperty TimeFormatProperty =
        DependencyProperty.Register(nameof(TimeFormat), typeof(string), typeof(TimelineControl),
            new PropertyMetadata("yyyy.MM.dd HH:mm"));

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
        set
        {
            SetValue(IsLockedProperty, value);
            ViewModel.IsLocked = value;
        }
    }

    public string TimeFormat
    {
        get => (string)GetValue(TimeFormatProperty);
        set => SetValue(TimeFormatProperty, value);
    }
    #endregion

    #region Constructor
    public TimelineControl()//(TimelineViewModel vm)
    {
        InitializeComponent();
        //ViewModel = vm;
        //DataContext = ViewModel;
        Loaded += TimelineControl_Loaded;
    }

    public void SetViewModel(TimelineViewModel vm)
    {
        ViewModel = vm;
        DataContext = vm;
    }
    #endregion

    #region Event Handlers
    private void TimelineControl_Loaded(object sender, RoutedEventArgs e)
    {
        contentGrid = FindChild<Grid>(this, "TimelineContentGrid");
        timelineCanvas = FindChild<Canvas>(this, "TimelineCanvas");
        markerCanvas = FindChild<Canvas>(this, "MarkerCanvas");
    }

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

    #region Drawing Methods
    private TextBlock? FindLabelFor(object tag) =>
        markerCanvas?.Children.OfType<TextBlock>()
            .FirstOrDefault(t => ReferenceEquals(t.Tag, tag));

    private Line? FindLineFor(object tag) =>
        markerCanvas?.Children.OfType<Line>()
            .FirstOrDefault(l => ReferenceEquals(l.Tag, tag));

    private Line? FindLineByTag(object? tag) =>
        markerCanvas?.Children
            .OfType<Line>()
            .FirstOrDefault(l => ReferenceEquals(l.Tag, tag));

    private bool IsSelectedLine(Line l) => ReferenceEquals(l, selectedLine);

    private void ApplySelectedLabelVisual(TextBlock label)
    {
        label.FontWeight = FontWeights.SemiBold;
        label.Foreground = SelectedBrush;
    }

    private void ResetLabelVisual(TextBlock label)
    {
        label.FontWeight = FontWeights.Normal;
        label.Foreground = MarkerBrush;
    }

    private void RedrawTimeline()
    {
        if (contentGrid == null || markerCanvas == null || timelineCanvas == null ||
            ViewModel?.Items == null || ViewModel.Items.Count == 0)
            return;

        contentGrid.Width = ViewModel.TimelineWidth;
        timelineCanvas.Width = ViewModel.TimelineWidth;

        markerCanvas.Children.Clear();
        selectedLine = null;

        var sorted = ViewModel.Items.OrderBy(i => i.Timestamp).ToList();

        foreach (var item in sorted)
        {
            var x = ViewModel.CalculateMarkerPosition(item.Timestamp);

            var marker = new Line
            {
                X1 = x,
                Y1 = 20,
                X2 = x,
                Y2 = 60,
                Stroke = MarkerBrush,
                StrokeThickness = 3,
                StrokeStartLineCap = PenLineCap.Round,
                StrokeEndLineCap = PenLineCap.Round,
                IsHitTestVisible = true,
                Tag = item
            };
            marker.PointerEntered += Marker_PointerEntered;
            marker.PointerExited += Marker_PointerExited;
            marker.Tapped += Marker_Tapped;
            markerCanvas.Children.Add(marker);

            var label = new TextBlock
            {
                Text = item.Timestamp.ToString("yy.MM.dd HH:mm:ss"),
                FontSize = 10,
                Foreground = MarkerBrush,
                Opacity = 0.9,
                IsHitTestVisible = true,
                TextAlignment = TextAlignment.Center,
                Tag = item
            };
            double labelWidth = 140;
            Canvas.SetLeft(label, x - labelWidth / 2);
            Canvas.SetTop(label, 57);
            label.Width = labelWidth;

            label.PointerEntered += Marker_PointerEntered;
            label.PointerExited += Marker_PointerExited;
            label.Tapped += Marker_Tapped;

            markerCanvas.Children.Add(label);

            AttachThumbnailTooltip(marker, item);
            AttachThumbnailTooltip(label, item);

            if (ViewModel.SelectedTimeLineItem != null && ReferenceEquals(item, ViewModel.SelectedTimeLineItem))
            {
                ApplySelectedVisual(marker);
            }
        }
    }

    private void UpdateSelectedVisual()
    {
        if (markerCanvas == null) 
            return;

        if (selectedLine != null)
        {
            selectedLine.Stroke = MarkerBrush;
            selectedLine.StrokeThickness = 3;

            var oldLbl = FindLabelFor(selectedLine.Tag!);
            if (oldLbl != null) 
                ResetLabelVisual(oldLbl);

            selectedLine = null;
        }

        if (ViewModel?.SelectedTimeLineItem == null) 
            return;

        var line = FindLineFor(ViewModel.SelectedTimeLineItem);
        if (line != null) 
            ApplySelectedVisual(line);
    }

    private void ApplySelectedVisual(Line line)
    {
        line.Stroke = SelectedBrush;
        line.StrokeThickness = 6;
        selectedLine = line;

        var lbl = FindLabelFor(line.Tag!);
        if (lbl != null)
            ApplySelectedLabelVisual(lbl);
    }

    private void AttachThumbnailTooltip(FrameworkElement target, TimelineItemDTO item)
    {
        var bmp = new BitmapImage();
        try
        {
            bmp.DecodePixelWidth = 220;
            bmp.UriSource = new Uri(item.ImagePath);
        }
        catch 
        { 
            return; 
        }

        var img = new Image
        {
            Source = bmp,
            Stretch = Stretch.Uniform,
            MaxWidth = 240,
            Margin = new Thickness(6)
        };

        var tt = new ToolTip
        {
            Content = img,
            Placement = Microsoft.UI.Xaml.Controls.Primitives.PlacementMode.Top
        };

        ToolTipService.SetToolTip(target, tt);
    }
    #endregion

    #region Marker Interaction
    private void Marker_PointerEntered(object sender, PointerRoutedEventArgs e)
    {
        if (IsLocked) return;

        var fe = sender as FrameworkElement;
        var line = sender as Line ?? FindLineByTag(fe?.Tag);
        if (line != null && !IsSelectedLine(line))
        {
            line.Stroke = HoverBrush;
            line.StrokeThickness = 5;
        }
    }

    private void Marker_PointerExited(object sender, PointerRoutedEventArgs e)
    {
        if (IsLocked) return;

        var fe = sender as FrameworkElement;
        var line = sender as Line ?? FindLineByTag(fe?.Tag);
        if (line != null && !IsSelectedLine(line))
        {
            line.Stroke = MarkerBrush;
            line.StrokeThickness = 3;
        }
    }

    private void Marker_Tapped(object sender, TappedRoutedEventArgs e)
    {
        if (IsLocked) return;

        var fe = sender as FrameworkElement;
        var item = fe?.Tag as TimelineItemDTO;
        if (item != null && ViewModel != null)
        {
            ViewModel.SelectedTimeLineItem = item;
        }
    }
    #endregion

    #region Helper Methods
    private T? FindChild<T>(DependencyObject parent, string childName) where T : DependencyObject
    {
        if (parent == null)
            return null;

        int count = VisualTreeHelper.GetChildrenCount(parent);
        for (int i = 0; i < count; i++)
        {
            var child = VisualTreeHelper.GetChild(parent, i);
            if (child is T t && (string)t.GetValue(FrameworkElement.NameProperty) == childName)
                return t;

            var result = FindChild<T>(child, childName);
            if (result != null)
                return result;
        }
        return null;
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