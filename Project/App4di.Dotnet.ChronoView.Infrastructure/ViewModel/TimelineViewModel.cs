/*
4di .NET ChronoView application
Copyright (c) 2025 by 4D Illusions. All rights reserved.
Released under the terms of the GNU General Public License version 3 or later.
*/

using App4di.Dotnet.ChronoView.Infrastructure.DTO;
using App4di.Dotnet.ChronoView.Infrastructure.Service;
using FW4di.Dotnet.MVVM;
using System.Collections.ObjectModel;

namespace App4di.Dotnet.ChronoView.Infrastructure.ViewModel;

public class TimelineViewModel : NotificationObject
{
    #region Constants
    private const double DefaultTargetWidthPx = 1600.0;
    private const double ZoomFactor = 1.5;
    private readonly string DateFormat = "yyyy.MM.dd HH:mm:ss";
    #endregion

    #region Commands
    public ICommand ZoomInCommand { get; }
    public ICommand ZoomOutCommand { get; }
    public ICommand ResetZoomCommand { get; }
    public ICommand CollapseExpandCommand { get; }
    #endregion

    #region Fields
    private double pixelsPerSecond;
    private double minPixelsPerSecond;
    private double maxPixelsPerSecond;
    private double defaultPixelsPerSecond;

    public event EventHandler<TimelineItemDTO?>? SelectedItemChanged;

    private readonly ISettingsService settings;
    #endregion

    #region Public props
    public ObservableCollection<TimelineItemDTO> Items
    {
        get;
        set
        {
            if (SetProperty(ref field, value))
            {
                field.CollectionChanged += (s, e) => OnItemsChanged();
                OnItemsChanged();
            }
        }
    } = new();

    public TimelineItemDTO? SelectedTimeLineItem
    {
        get;
        set
        {
            if (SetProperty(ref field, value))
            {
                RaisePropertyChanged(nameof(SelectedImage));

                SelectedItemChanged?.Invoke(this, value);

                (ZoomInCommand as RelayCommand)?.RaiseCanExecuteChanged();
                (ZoomOutCommand as RelayCommand)?.RaiseCanExecuteChanged();
                (ResetZoomCommand as RelayCommand)?.RaiseCanExecuteChanged();
            }
        }
    }

    public double AbsoluteMinPps
    {
        get;
        set => SetProperty(ref field, value);
    } = 0.00001;

    public double AbsoluteMaxPps
    {
        get;
        set => SetProperty(ref field, value);
    } = 10000.0;

    public double PixelsPerSecond
    {
        get => pixelsPerSecond;
        set
        {
            var clamped = Math.Clamp(value, MinPixelsPerSecond, MaxPixelsPerSecond);
            if (SetProperty(ref pixelsPerSecond, clamped))
            {
                CalculateTimelineMetrics();
                RiseAllButtonsExecuteChanged();
            }
        }
    }

    public string CollapseExpandGlyph => IsCollapsed ? "\uE70E" : "\uE70D";
    public bool IsCollapsed
    {
        get => settings.IsTimelineCollapsed;
        
        set
        {
            settings.IsTimelineCollapsed = value;
            RaisePropertyChanged(nameof(IsCollapsed), nameof(CollapseExpandGlyph));
        }
    }

    public double MinPixelsPerSecond
    {
        get => minPixelsPerSecond;
        private set => SetProperty(ref minPixelsPerSecond, Math.Clamp(value, AbsoluteMinPps, AbsoluteMaxPps));
    }

    public double MaxPixelsPerSecond
    {
        get => maxPixelsPerSecond;
        private set => SetProperty(ref maxPixelsPerSecond, Math.Clamp(value, AbsoluteMinPps, AbsoluteMaxPps));
    }

    public double DefaultPixelsPerSecond
    {
        get => defaultPixelsPerSecond;
        private set => SetProperty(ref defaultPixelsPerSecond, Math.Clamp(value, AbsoluteMinPps, AbsoluteMaxPps));
    }

    public double TargetWidthPx
    {
        get;
        set
        {
            if (SetProperty(ref field, Math.Max(300, value)))
            {
                RecomputePpsBoundsAndMaybeReset();
            }
        }
    } = DefaultTargetWidthPx;

    public double TimelineWidth
    {
        get;
        set => SetProperty(ref field, value);
    }

    public string StartDateText
    {
        get;
        set => SetProperty(ref field, value);
    } = string.Empty;

    public string EndDateText
    {
        get;
        set => SetProperty(ref field, value);
    } = string.Empty;

    public string SelectedImage => SelectedTimeLineItem?.ImageName + " [" + SelectedTimeLineItem?.Timestamp + "]" ?? string.Empty;

    public DateTime MinTimestamp
    {
        get;
        private set => SetProperty(ref field, value);
    }

    public DateTime MaxTimestamp
    {
        get;
        private set => SetProperty(ref field, value);
    }

    public int RedrawTrigger
    {
        get;
        private set => SetProperty(ref field, value);
    }

    public bool IsLocked
    {
        get;
        set
        {
            SetProperty(ref field, value);
            RiseAllButtonsExecuteChanged();
        }
    }
    #endregion

    #region CTor
    public TimelineViewModel(ISettingsService settings)
    {
        this.settings = settings ?? throw new ArgumentNullException(nameof(settings));

        IsCollapsed = settings.IsTimelineCollapsed;

        MinPixelsPerSecond = 0.001;
        MaxPixelsPerSecond = 10.0;
        DefaultPixelsPerSecond = 0.02;
        pixelsPerSecond = DefaultPixelsPerSecond;
        
        ZoomInCommand = new RelayCommand(_ => ZoomIn(), _ => !IsLocked && SelectedTimeLineItem != null && PixelsPerSecond < MaxPixelsPerSecond);
        ZoomOutCommand = new RelayCommand(_ => ZoomOut(), _ => !IsLocked && SelectedTimeLineItem != null && PixelsPerSecond > MinPixelsPerSecond);
        ResetZoomCommand = new RelayCommand(_ => ResetZoom(), _ => !IsLocked && SelectedTimeLineItem != null && Math.Abs(PixelsPerSecond - DefaultPixelsPerSecond) > 0.0000001);
        CollapseExpandCommand = new RelayCommand(_ => CollapseExpand(), _ => true);

        Items.CollectionChanged += (s, e) => OnItemsChanged();
    }
    #endregion

    #region Functions
    void RiseAllButtonsExecuteChanged()
    {
        (ZoomInCommand as RelayCommand)?.RaiseCanExecuteChanged();
        (ZoomOutCommand as RelayCommand)?.RaiseCanExecuteChanged();
        (ResetZoomCommand as RelayCommand)?.RaiseCanExecuteChanged();
    }

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

    private void CollapseExpand()
    {
        IsCollapsed = !IsCollapsed;
    }

    private void OnItemsChanged()
    {
        CalculateTimelineMetrics();
        RecomputePpsBoundsAndMaybeReset(forceResetToDefault: true);
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

    private void RecomputePpsBoundsAndMaybeReset(bool forceResetToDefault = false)
    {
        if (Items == null || Items.Count == 0 || MinTimestamp == DateTime.MinValue || MaxTimestamp == DateTime.MinValue)
            return;

        var totalSeconds = Math.Max((MaxTimestamp - MinTimestamp).TotalSeconds, 1);

        var desiredContentWidth = Math.Max(200.0, TargetWidthPx - 100.0);
        var computedDefault = desiredContentWidth / totalSeconds;

        var computedMin = computedDefault / 20.0;
        var computedMax = computedDefault * 40.0;

        DefaultPixelsPerSecond = Math.Clamp(computedDefault, AbsoluteMinPps, AbsoluteMaxPps);
        MinPixelsPerSecond = Math.Clamp(computedMin, AbsoluteMinPps, DefaultPixelsPerSecond);
        MaxPixelsPerSecond = Math.Clamp(computedMax, DefaultPixelsPerSecond, AbsoluteMaxPps);

        if (forceResetToDefault || PixelsPerSecond < MinPixelsPerSecond || PixelsPerSecond > MaxPixelsPerSecond)
        {
            pixelsPerSecond = DefaultPixelsPerSecond;
            RaisePropertyChanged(nameof(PixelsPerSecond));
            CalculateTimelineMetrics();
        }

        RiseAllButtonsExecuteChanged();
    }
    #endregion
}
