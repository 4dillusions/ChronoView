/*
4di .NET ChronoView application
Copyright (c) 2025 by 4D Illusions. All rights reserved.
Released under the terms of the GNU General Public License version 3 or later.
*/

using App4di.Dotnet.ChronoView.Infrastructure.DTO;
using FW4di.Dotnet.MVVM;
using System.Collections.ObjectModel;

namespace App4di.Dotnet.ChronoView.Infrastructure.ViewModel;

public class TimelineViewModel : NotificationObject
{
    #region Constants
    private const double MinPixelsPerSecond = 0.1;
    private const double MaxPixelsPerSecond = 50.0;
    private const double DefaultPixelsPerSecond = 2.0;
    private const double ZoomFactor = 1.5;
    private readonly string DateFormat = "yyyy.MM.dd HH:mm:ss";
    #endregion

    #region Commands
    public ICommand ZoomInCommand { get; }
    public ICommand ZoomOutCommand { get; }
    public ICommand ResetZoomCommand { get; }
    #endregion

    #region Fields
    private ObservableCollection<TimelineItemDTO> items = new();
    private TimelineItemDTO? selectedTimeLineItem;
    private double pixelsPerSecond = DefaultPixelsPerSecond;
    private double timelineWidth;
    private string startDateText = string.Empty;
    private string endDateText = string.Empty;
    private DateTime minTimestamp;
    private DateTime maxTimestamp;
    private int redrawTrigger;

    public event EventHandler<TimelineItemDTO?>? SelectedItemChanged;

    public ObservableCollection<TimelineItemDTO> Items
    {
        get => items;
        set
        {
            if (SetProperty(ref items, value))
            {
                items.CollectionChanged += (s, e) => OnItemsChanged();
                OnItemsChanged();
            }
        }
    }

    public TimelineItemDTO? SelectedTimeLineItem
    {
        get => selectedTimeLineItem;
        set
        {
            if (SetProperty(ref selectedTimeLineItem, value))
            {
                RaisePropertyChanged(nameof(SelectedImage));
                
                SelectedItemChanged?.Invoke(this, value);
            }
        }
    }

    public double PixelsPerSecond
    {
        get => pixelsPerSecond;
        set
        {
            if (SetProperty(ref pixelsPerSecond, value))
            {
                CalculateTimelineMetrics();

                (ZoomInCommand as RelayCommand)?.RaiseCanExecuteChanged();
                (ZoomOutCommand as RelayCommand)?.RaiseCanExecuteChanged();
                (ResetZoomCommand as RelayCommand)?.RaiseCanExecuteChanged();
            }
        }
    }

    public double TimelineWidth
    {
        get => timelineWidth;
        set => SetProperty(ref timelineWidth, value);
    }

    public string StartDateText
    {
        get => startDateText;
        set => SetProperty(ref startDateText, value);
    }

    public string EndDateText
    {
        get => endDateText;
        set => SetProperty(ref endDateText, value);
    }

    public string SelectedImage => SelectedTimeLineItem?.ImageName + " [" + SelectedTimeLineItem?.Timestamp + "]" ?? string.Empty;

    public DateTime MinTimestamp
    {
        get => minTimestamp;
        private set => SetProperty(ref minTimestamp, value);
    }

    public DateTime MaxTimestamp
    {
        get => maxTimestamp;
        private set => SetProperty(ref maxTimestamp, value);
    }

    public int RedrawTrigger
    {
        get => redrawTrigger;
        private set => SetProperty(ref redrawTrigger, value);
    }
    #endregion

    #region Constructor
    public TimelineViewModel()
    {
        ZoomInCommand = new RelayCommand(_ => ZoomIn(), _ => PixelsPerSecond < MaxPixelsPerSecond);
        ZoomOutCommand = new RelayCommand(_ => ZoomOut(), _ => PixelsPerSecond > MinPixelsPerSecond);
        ResetZoomCommand = new RelayCommand(_ => ResetZoom(), _ => Math.Abs(PixelsPerSecond - DefaultPixelsPerSecond) > 0.01);

        Items.CollectionChanged += (s, e) => OnItemsChanged();
    }
    #endregion

    #region Methods
    private void ZoomIn()
    {
        PixelsPerSecond = Math.Min(PixelsPerSecond * ZoomFactor, MaxPixelsPerSecond);
    }

    private void ZoomOut()
    {
        PixelsPerSecond = Math.Max(PixelsPerSecond / ZoomFactor, MinPixelsPerSecond);
    }

    private void ResetZoom()
    {
        PixelsPerSecond = DefaultPixelsPerSecond;
    }

    private void OnItemsChanged()
    {
        CalculateTimelineMetrics();
    }

    private void CalculateTimelineMetrics()
    {
        if (Items == null || Items.Count == 0)
        {
            TimelineWidth = 0;
            StartDateText = string.Empty;
            EndDateText = string.Empty;
            MinTimestamp = DateTime.MinValue;
            MaxTimestamp = DateTime.MinValue;

            return;
        }

        var sorted = Items.OrderBy(i => i.Timestamp).ToList();
        MinTimestamp = sorted.First().Timestamp;
        MaxTimestamp = sorted.Last().Timestamp;

        var totalSeconds = Math.Max((MaxTimestamp - MinTimestamp).TotalSeconds, 1);
        TimelineWidth = totalSeconds * PixelsPerSecond + 100;

        StartDateText = MinTimestamp.ToString(DateFormat);
        EndDateText = MaxTimestamp.ToString(DateFormat);

        RedrawTrigger++;
    }

    public double CalculateMarkerPosition(DateTime timestamp)
    {
        if (Items == null || Items.Count == 0)
            return 0;

        return 50 + (timestamp - MinTimestamp).TotalSeconds * PixelsPerSecond;
    }
    #endregion
}
