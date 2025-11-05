/*
4di .NET ChronoView application
Copyright (c) 2025 by 22 by 4D Illusions. All rights reserved.
Released under the terms of the GNU General Public License version 3 or later.
*/

using App4di.Dotnet.ChronoView.Infrastructure.DTOs;
using Microsoft.UI;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Shapes;
using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using Windows.System;

namespace App4di.Dotnet.ChronoView.WinUI.Controls;

public sealed partial class TimelineControl : UserControl
{
    private Canvas? markerCanvas;
    private Canvas? timelineCanvas;
    private Grid? contentGrid;
    private TextBlock? startDateText;
    private TextBlock? endDateText;
    private Button? zoomInBtn;
    private Button? zoomOutBtn;
    private Button? resetZoomBtn;

    private readonly string DateFormat = "yyyy.MM.dd HH:mm:ss";

    private double pixelsPerSecond = 2.0;
    private const double MinPixelsPerSecond = 0.1;
    private const double MaxPixelsPerSecond = 50.0;

    public static readonly DependencyProperty ItemsProperty =
        DependencyProperty.Register(nameof(Items), typeof(ObservableCollection<TimelineItemDTO>), typeof(TimelineControl),
            new PropertyMetadata(null, OnItemsChanged));

    public static readonly DependencyProperty SelectedTimeLineItemProperty =
       DependencyProperty.Register(nameof(SelectedTimeLineItem), typeof(TimelineItemDTO), typeof(TimelineControl),
           new PropertyMetadata(null, OnSelectedItemChanged));

    public static readonly DependencyProperty MarkerBrushProperty =
        DependencyProperty.Register(nameof(MarkerBrush), typeof(Brush), typeof(TimelineControl),
            new PropertyMetadata(new SolidColorBrush(Colors.DodgerBlue), OnBrushChanged));

    public static readonly DependencyProperty HoverBrushProperty =
        DependencyProperty.Register(nameof(HoverBrush), typeof(Brush), typeof(TimelineControl),
            new PropertyMetadata(new SolidColorBrush(Colors.Black), OnBrushChanged));

    public static readonly DependencyProperty SelectedBrushProperty =
        DependencyProperty.Register(nameof(SelectedBrush), typeof(Brush), typeof(TimelineControl),
            new PropertyMetadata(new SolidColorBrush(Colors.White), OnBrushChanged));

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

    private Line? selectedLine;

    public ObservableCollection<TimelineItemDTO> Items
    {
        get => (ObservableCollection<TimelineItemDTO>)GetValue(ItemsProperty);
        set => SetValue(ItemsProperty, value);
    }

    public TimelineItemDTO SelectedTimeLineItem
    {
        get => (TimelineItemDTO)GetValue(SelectedTimeLineItemProperty);
        set => SetValue(SelectedTimeLineItemProperty, value);
    }

    public event EventHandler<TimelineItemDTO>? SelectedItemChanged;

    public TimelineControl()
    {
        InitializeComponent();
        Items = new ObservableCollection<TimelineItemDTO>();
        Loaded += TimelineControl_Loaded;
    }

    private void TimelineControl_Loaded(object sender, RoutedEventArgs e)
    {
        contentGrid = FindChild<Grid>(this, "TimelineContentGrid");
        timelineCanvas = FindChild<Canvas>(this, "TimelineCanvas");
        markerCanvas = FindChild<Canvas>(this, "MarkerCanvas");
        startDateText = FindChild<TextBlock>(this, "StartDateText");
        endDateText = FindChild<TextBlock>(this, "EndDateText");
        zoomInBtn = FindChild<Button>(this, "ZoomInBtn");
        zoomOutBtn = FindChild<Button>(this, "ZoomOutBtn");
        resetZoomBtn = FindChild<Button>(this, "ResetZoomBtn");

        if (zoomInBtn != null) 
            zoomInBtn.Click += ZoomInBtn_Click;
        
        if (zoomOutBtn != null) 
            zoomOutBtn.Click += ZoomOutBtn_Click;
        
        if (resetZoomBtn != null) 
            resetZoomBtn.Click += ResetZoomBtn_Click;
    }

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

    private static void OnItemsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not TimelineControl control) 
            return;

        if (e.OldValue is ObservableCollection<TimelineItemDTO> old) 
            old.CollectionChanged -= control.OnCollectionChanged;

        if (e.NewValue is ObservableCollection<TimelineItemDTO> newColl) 
            newColl.CollectionChanged += control.OnCollectionChanged;

        control.DispatcherQueue.TryEnqueue(control.RedrawTimeline);
    }

    public void RedrawTimeline()
    {
        if (contentGrid == null || markerCanvas == null || Items == null || Items.Count == 0 || timelineCanvas == null)
            return;

        var sorted = Items.OrderBy(i => i.Timestamp).ToList();
        var min = sorted.First().Timestamp;
        var max = sorted.Last().Timestamp;
        var totalSec = Math.Max((max - min).TotalSeconds, 1);
        var width = totalSec * pixelsPerSecond + 100;

        contentGrid.Width = width;
        timelineCanvas.Width = width;

        markerCanvas.Children.Clear();
        selectedLine = null;

        if (startDateText != null) 
            startDateText.Text = min.ToString(DateFormat);

        if (endDateText != null) 
            endDateText.Text = max.ToString(DateFormat);

        foreach (var item in sorted)
        {
            var x = 50 + (item.Timestamp - min).TotalSeconds * pixelsPerSecond;

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

            if (SelectedTimeLineItem != null && ReferenceEquals(item, SelectedTimeLineItem))
                ApplySelectedVisual(marker);
        }
    }


    private void ChangeZoom(double factor)
    {
        pixelsPerSecond = Math.Clamp(pixelsPerSecond * factor, MinPixelsPerSecond, MaxPixelsPerSecond);
        RedrawTimeline();
    }

    private bool IsSelectedLine(Line line) => selectedLine == line;

    public void ZoomIn() => ChangeZoom(1.5);
    public void ZoomOut() => ChangeZoom(1.0 / 1.5);
    public void ResetZoom() { pixelsPerSecond = 2.0; RedrawTimeline(); }

    private void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e) => DispatcherQueue.TryEnqueue(RedrawTimeline);

    private void ZoomInBtn_Click(object sender, RoutedEventArgs e) => ZoomIn();
    private void ZoomOutBtn_Click(object sender, RoutedEventArgs e) => ZoomOut();
    private void ResetZoomBtn_Click(object sender, RoutedEventArgs e) => ResetZoom();

    private void Marker_PointerEntered(object sender, PointerRoutedEventArgs e)
    {
        if (sender is Line line && !IsSelectedLine(line))
        {
            line.Stroke = HoverBrush;
            line.StrokeThickness = 5;
        }
    }

    private void Marker_PointerExited(object sender, PointerRoutedEventArgs e)
    {
        if (sender is Line line && !IsSelectedLine(line))
        {
            line.Stroke = MarkerBrush;
            line.StrokeThickness = 3;
        }
    }

    private static void OnSelectedItemChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var control = (TimelineControl)d;
        control.UpdateSelectedVisual(e.NewValue as TimelineItemDTO);
        if (e.NewValue is TimelineItemDTO dto)
            control.SelectedItemChanged?.Invoke(control, dto);
    }

    private void Marker_Tapped(object sender, TappedRoutedEventArgs e)
    {
        if (sender is Line line && line.Tag is TimelineItemDTO item)
        {
            SelectedTimeLineItem = item;
        }
    }

    private void UpdateSelectedVisual(TimelineItemDTO? item)
    {
        if (markerCanvas == null) 
            return;

        if (selectedLine != null)
        {
            selectedLine.Stroke = MarkerBrush;
            selectedLine.StrokeThickness = 3;
            selectedLine = null;
        }

        if (item == null) 
            return;

        var line = markerCanvas.Children.OfType<Line>().FirstOrDefault(l => ReferenceEquals(l.Tag, item));
        if (line != null)
            ApplySelectedVisual(line);
    }

    private void ApplySelectedVisual(Line line)
    {
        line.Stroke = SelectedBrush;
        line.StrokeThickness = 6;
        selectedLine = line;
    }

    private static void OnBrushChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        (d as TimelineControl)?.RedrawTimeline();
    }
}
