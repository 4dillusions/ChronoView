/*
4di .NET ChronoView application
Copyright (c) 2025 by 4D Illusions. All rights reserved.
Released under the terms of the GNU General Public License version 3 or later.
*/

using App4di.Dotnet.ChronoView.Infrastructure.DTO;
using App4di.Dotnet.ChronoView.Infrastructure.Service;
using FW4di.Dotnet.MVVM;
using FW4di.Dotnet.MVVM.Service;
using System.Collections.ObjectModel;

namespace App4di.Dotnet.ChronoView.Infrastructure.ViewModel;

public class HomeViewModel : NotificationObject
{
    #region Commands
    public ICommand BackCommand { get; }
    public ICommand NextCommand { get; }
    public ICommand PlayPauseCommand { get; }
    public ICommand ZoomInCommand { get; }
    public ICommand ZoomOutCommand { get; }
    public ICommand ResetZoomCommand { get; }
    public ICommand RotateCommand { get; }
    public ICommand OpenFolderCommand { get; }
    public ICommand ImageOpenedCommand { get; }
    #endregion

    #region Fields
    private readonly IFolderPickerService folderPicker;
    private readonly IFileService file;
    private readonly ISettingsService settings;

    public int TimelineRowHeight
    {
        get;
        set => SetProperty(ref field, value);
    } = 200;
    
    public float ZoomFactor
    {
        get;
        set
        {
            if (SetProperty(ref field, value))
            {
                (ZoomInCommand as RelayCommand)?.RaiseCanExecuteChanged();
                (ZoomOutCommand as RelayCommand)?.RaiseCanExecuteChanged();
                (ResetZoomCommand as RelayCommand)?.RaiseCanExecuteChanged();
            }
        }
    } = 1.0f;

    public double CurrentRotationAngle
    {
        get;
        set => SetProperty(ref field, value);
    } = 0;

    public double TargetRotationAngle
    {
        get;
        set => SetProperty(ref field, value);
    } = 0;

    public TimelineItemDTO? SelectedImageItem
    {
        get;

        set
        {
            if (SetProperty(ref field, value))
            {
                RiseAllButtonsExecuteChanged();

                if (TimelineItems != null && TimelineItems.Count > 0 && field != null)
                {
                    var idx = TimelineItems.IndexOf(field);
                    if (idx >= 0 && idx != SelectedIndex)
                        SelectedIndex = idx;
                }
            }

            Reset();
        }
    }

    public ObservableCollection<TimelineItemDTO> TimelineItems
    {
        get;
        set => SetProperty(ref field, value);
    } = new ();

    public string PlayPauseGlyph => IsAutoPlay ? "\uE769" : "\uE768";

    public int SelectedIndex
    {
        get;
        set
        {
            if (SetProperty(ref field, value))
            {
                if (TimelineItems != null && TimelineItems.Count > 0)
                {
                    if (field < 0)
                        field = 0;

                    if (field > TimelineItems.Count - 1)
                        field = TimelineItems.Count - 1;

                    var newItem = TimelineItems[field];
                    if (!ReferenceEquals(SelectedImageItem, newItem))
                        SelectedImageItem = newItem;
                }
            }
        }
    } = -1;

    public bool IsAutoPlay
    {
        get;
        private set
        {
            if (SetProperty(ref field, value))
            {
                RaisePropertyChanged(nameof(IsAutoPlay), nameof(PlayPauseGlyph));
            }
        }
    }

    public double ViewportWidth
    {
        get;
        set
        {
            if (SetProperty(ref field, value))
            {
                CalculateFitToViewport();
            }
        }
    }

    public double ViewportHeight
    {
        get;
        set
        {
            if (SetProperty(ref field, value))
            {
                CalculateFitToViewport();
            }
        }
    }

    public double ImageWidth
    {
        get;
        set
        {
            if (SetProperty(ref field, value))
            {
                CalculateFitToViewport();
            }
        }
    }

    public double ImageHeight
    {
        get;
        set
        {
            if (SetProperty(ref field, value))
            {
                CalculateFitToViewport();
            }
        }
    }

    public bool ShouldFitToViewport
    {
        get;
        set => SetProperty(ref field, value);
    }

    bool IsBackCanExecute
    {
        get
        {
            if (!IsAutoPlay && TimelineItems.Count > 0 && SelectedImageItem != null)
                return TimelineItems.IndexOf(SelectedImageItem) > 0;

            return false;
        }
    }

    public bool IsNextCanExecute
    {
        get
        {
            if (!IsAutoPlay && TimelineItems.Count > 0 && SelectedImageItem != null)
                return TimelineItems.IndexOf(SelectedImageItem) < TimelineItems.Count - 1;

            return false;
        }
    }

    bool IsPlayPauseCanExecute => TimelineItems.Count > 1 && SelectedImageItem != null;
    #endregion

    #region CTor
    public HomeViewModel(IFolderPickerService folderPicker, IFileService file, ISettingsService settings)
    {
        this.folderPicker = folderPicker ?? throw new ArgumentNullException(nameof(folderPicker));
        this.file = file ?? throw new ArgumentNullException(nameof(file));
        this.settings = settings ?? throw new ArgumentNullException(nameof(settings));

        BackCommand = new RelayCommand(_ => Back(), _ => IsBackCanExecute);
        NextCommand = new RelayCommand(_ => Next(), _ => IsNextCanExecute);
        PlayPauseCommand = new RelayCommand(_ => PlayPause(), _ => IsPlayPauseCanExecute);
        ZoomInCommand = new RelayCommand(_ => ZoomIn(), _ => SelectedImageItem != null && ZoomFactor < settings.MaxZoom && !IsAutoPlay);
        ZoomOutCommand = new RelayCommand(_ => ZoomOut(), _ => SelectedImageItem != null && ZoomFactor > settings.MinZoom && !IsAutoPlay);
        ResetZoomCommand = new RelayCommand(_ => ResetZoom(), _ => SelectedImageItem != null && ZoomFactor != 1.0f && !IsAutoPlay);
        RotateCommand = new RelayCommand(_ => Rotate(), _ => SelectedImageItem != null && !IsAutoPlay);
        OpenFolderCommand = new RelayCommand(_ => OpenFolder(), _ => !IsAutoPlay);
        ImageOpenedCommand = new RelayCommand(_ => OnImageOpened());

        if (TimelineItems != null && TimelineItems.Count > 0 && SelectedIndex < 0)
            SelectedIndex = 0;
    }
    #endregion

    #region Functions
    public void GoToFirst()
    {
        SelectedIndex = 0;
        RaisePropertyChanged(nameof(SelectedIndex));
    }

    void RiseAllButtonsExecuteChanged()
    {
        (OpenFolderCommand as RelayCommand)?.RaiseCanExecuteChanged();
        (BackCommand as RelayCommand)?.RaiseCanExecuteChanged();
        (NextCommand as RelayCommand)?.RaiseCanExecuteChanged();
        (PlayPauseCommand as RelayCommand)?.RaiseCanExecuteChanged();
        (ZoomInCommand as RelayCommand)?.RaiseCanExecuteChanged();
        (ZoomOutCommand as RelayCommand)?.RaiseCanExecuteChanged();
        (ResetZoomCommand as RelayCommand)?.RaiseCanExecuteChanged();
        (RotateCommand as RelayCommand)?.RaiseCanExecuteChanged();
    }

    void Back()
    {
        if (TimelineItems == null || TimelineItems.Count == 0)
            return;

        if (SelectedIndex < 0)
            SelectedIndex = 0;
        else
            SelectedIndex = (SelectedIndex - 1 + TimelineItems.Count) % TimelineItems.Count;
    }

    void Next()
    {
        if (TimelineItems == null || TimelineItems.Count == 0)
            return;

        if (SelectedIndex < 0)
            SelectedIndex = 0;
        else
            SelectedIndex = (SelectedIndex + 1) % TimelineItems.Count;
    }

    void PlayPause()
    {
        if (TimelineItems == null || TimelineItems.Count <= 1)
            return;

        IsAutoPlay = !IsAutoPlay;
        RiseAllButtonsExecuteChanged();
    }

    void ZoomIn()
    {
        ZoomFactor = Math.Min(ZoomFactor * settings.ZoomStep, settings.MaxZoom);
    }

    void ZoomOut()
    {
        ZoomFactor = Math.Max(ZoomFactor / settings.ZoomStep, settings.MinZoom);
    }

    void ResetZoom()
    {
        CalculateFitToViewport();
    }

    void Rotate()
    {
        CurrentRotationAngle = TargetRotationAngle;
        TargetRotationAngle += 90;

        if (TargetRotationAngle >= 360)
            TargetRotationAngle = 0;

        CalculateFitToViewport();
    }

    public async Task OpenFolder()
    {
        var path = await folderPicker.PickFolderAsync();
        if (path != null)
        {
            TimelineItems = new ObservableCollection<TimelineItemDTO>(file.LoadImagesFromFolder(path, isAllFoldersRecursive: false, extensions: [".jpg", /*".png"*/]));

            if (TimelineItems.Count > 0)
            {
                SelectedImageItem = TimelineItems[0];
            }
        }
    }

    void Reset()
    {
        ResetZoom();
        TargetRotationAngle = 0;
    }

    void OnImageOpened()
    {
        CalculateFitToViewport();
    }

    void CalculateFitToViewport()
    {
        if (ViewportWidth <= 0 || ViewportHeight <= 0 || ImageWidth <= 0 || ImageHeight <= 0)
            return;

        double imgW = Math.Max(1, ImageWidth);
        double imgH = Math.Max(1, ImageHeight);

        var angle = TargetRotationAngle % 360;
        if (angle < 0)
            angle += 360;

        bool swap = angle == 90 || angle == 270;
        if (swap)
        {
            var t = imgW;
            imgW = imgH;
            imgH = t;
        }

        double vpW = ViewportWidth;
        double vpH = ViewportHeight;

        var scale = Math.Min(vpW / imgW, vpH / imgH);
        scale = Math.Clamp(scale, settings.MinZoom, settings.MaxZoom);

        ZoomFactor = (float)scale;
        ShouldFitToViewport = true;
    }
    #endregion
}
