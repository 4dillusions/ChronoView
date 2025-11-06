/*
4di .NET ChronoView application
Copyright (c) 2025 by 4D Illusions. All rights reserved.
Released under the terms of the GNU General Public License version 3 or later.
*/

using App4di.Dotnet.ChronoView.Infrastructure.DTO;
using App4di.Dotnet.ChronoView.Infrastructure.ViewModel;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Shapes;
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
    #endregion

    #region Constructor
    public TimelineControl()
    {
        InitializeComponent();
        ViewModel = new TimelineViewModel();
        DataContext = ViewModel;
        Loaded += TimelineControl_Loaded;
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

            if (ViewModel.SelectedTimeLineItem != null &&
                ReferenceEquals(item, ViewModel.SelectedTimeLineItem))
            {
                ApplySelectedVisual(marker);
            }
        }
    }

    private void UpdateSelectedVisual()
    {
        if (markerCanvas == null)
            return;

        // Reset previous selection
        if (selectedLine != null)
        {
            selectedLine.Stroke = MarkerBrush;
            selectedLine.StrokeThickness = 3;
            selectedLine = null;
        }

        if (ViewModel?.SelectedTimeLineItem == null)
            return;

        // Apply new selection
        var line = markerCanvas.Children.OfType<Line>()
            .FirstOrDefault(l => ReferenceEquals(l.Tag, ViewModel.SelectedTimeLineItem));

        if (line != null)
            ApplySelectedVisual(line);
    }

    private void ApplySelectedVisual(Line line)
    {
        line.Stroke = SelectedBrush;
        line.StrokeThickness = 6;
        selectedLine = line;
    }
    #endregion

    #region Marker Interaction
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

    private void Marker_Tapped(object sender, TappedRoutedEventArgs e)
    {
        if (sender is Line line && line.Tag is TimelineItemDTO item && ViewModel != null)
        {
            ViewModel.SelectedTimeLineItem = item;
        }
    }

    private bool IsSelectedLine(Line line) => selectedLine == line;
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
        set { if (ViewModel != null) ViewModel.Items = value; }
    }

    public TimelineItemDTO? SelectedTimeLineItem
    {
        get => ViewModel?.SelectedTimeLineItem;
        set { if (ViewModel != null) ViewModel.SelectedTimeLineItem = value; }
    }

    public void ZoomIn() => ViewModel?.ZoomInCommand?.Execute(null);
    public void ZoomOut() => ViewModel?.ZoomOutCommand?.Execute(null);
    public void ResetZoom() => ViewModel?.ResetZoomCommand?.Execute(null);
    #endregion
}