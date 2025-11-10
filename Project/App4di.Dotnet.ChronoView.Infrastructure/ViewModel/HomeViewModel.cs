/*
4di .NET ChronoView application
Copyright (c) 2025 by 4D Illusions. All rights reserved.
Released under the terms of the GNU General Public License version 3 or later.
*/

using App4di.Dotnet.ChronoView.Infrastructure.DTO;
using App4di.Dotnet.ChronoView.Infrastructure.Helper;
using App4di.Dotnet.ChronoView.Infrastructure.Service;
using FW4di.Dotnet.MVVM;
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
    float zoomFactor = 1.0f;
    double currentRotationAngle = 0;
    double targetRotationAngle = 0;
    TimelineItemDTO? selectedImageItem;
    ObservableCollection<TimelineItemDTO> timelineItems = new();
    private int selectedIndex = -1;
    private bool isAutoPlay;
    private double viewportWidth;
    private double viewportHeight;
    private double imageWidth;
    private double imageHeight;
    private bool shouldFitToViewport;

    private readonly IFolderPickerService folderPicker;
    private readonly FileService file;

    public float ZoomFactor
    {
        get => zoomFactor;
        set
        {
            if (SetProperty(ref zoomFactor, value))
            {
                (ZoomInCommand as RelayCommand)?.RaiseCanExecuteChanged();
                (ZoomOutCommand as RelayCommand)?.RaiseCanExecuteChanged();
                (ResetZoomCommand as RelayCommand)?.RaiseCanExecuteChanged();
            }
        }
    }

    public double CurrentRotationAngle
    {
        get => currentRotationAngle;
        set => SetProperty(ref currentRotationAngle, value);
    }

    public double TargetRotationAngle
    {
        get => targetRotationAngle;
        set => SetProperty(ref targetRotationAngle, value);
    }

    public TimelineItemDTO? SelectedImageItem
    {
        get => selectedImageItem;

        set
        {
            if (SetProperty(ref selectedImageItem, value))
            {
                RiseAllButtonsExecuteChanged();

                if (TimelineItems != null && TimelineItems.Count > 0 && selectedImageItem != null)
                {
                    var idx = TimelineItems.IndexOf(selectedImageItem);
                    if (idx >= 0 && idx != SelectedIndex)
                        SelectedIndex = idx;
                }
            }

            Reset();
        }
    }

    public ObservableCollection<TimelineItemDTO> TimelineItems
    {
        get => timelineItems;
        set => SetProperty(ref timelineItems, value);
    }

    public string PlayPauseGlyph => IsAutoPlay ? "\uE769" : "\uE768";

    public int SelectedIndex
    {
        get => selectedIndex;
        set
        {
            if (SetProperty(ref selectedIndex, value))
            {
                if (TimelineItems != null && TimelineItems.Count > 0)
                {
                    if (selectedIndex < 0)
                        selectedIndex = 0;
                    if (selectedIndex > TimelineItems.Count - 1)
                        selectedIndex = TimelineItems.Count - 1;

                    var newItem = TimelineItems[selectedIndex];
                    if (!ReferenceEquals(SelectedImageItem, newItem))
                        SelectedImageItem = newItem;
                }
            }
        }
    }

    public bool IsAutoPlay
    {
        get => isAutoPlay;
        private set
        {
            if (SetProperty(ref isAutoPlay, value))
            {
                RaisePropertyChanged(nameof(IsAutoPlay), nameof(PlayPauseGlyph));
            }
        }
    }

    public double ViewportWidth
    {
        get => viewportWidth;
        set
        {
            if (SetProperty(ref viewportWidth, value))
            {
                CalculateFitToViewport();
            }
        }
    }

    public double ViewportHeight
    {
        get => viewportHeight;
        set
        {
            if (SetProperty(ref viewportHeight, value))
            {
                CalculateFitToViewport();
            }
        }
    }

    public double ImageWidth
    {
        get => imageWidth;
        set
        {
            if (SetProperty(ref imageWidth, value))
            {
                CalculateFitToViewport();
            }
        }
    }

    public double ImageHeight
    {
        get => imageHeight;
        set
        {
            if (SetProperty(ref imageHeight, value))
            {
                CalculateFitToViewport();
            }
        }
    }

    public bool ShouldFitToViewport
    {
        get => shouldFitToViewport;
        set => SetProperty(ref shouldFitToViewport, value);
    }

    bool IsBackCanExecute
    {
        get
        {
            if (!IsAutoPlay && TimelineItems.Count > 0 && selectedImageItem != null)
                return TimelineItems.IndexOf(selectedImageItem) > 0;

            return false;
        }
    }

    public bool IsNextCanExecute
    {
        get
        {
            if (!IsAutoPlay && TimelineItems.Count > 0 && selectedImageItem != null)
                return TimelineItems.IndexOf(selectedImageItem) < TimelineItems.Count - 1;

            return false;
        }
    }

    bool IsPlayPauseCanExecute => TimelineItems.Count > 1 && selectedImageItem != null;
    #endregion

    #region CTor
    public HomeViewModel(IFolderPickerService folderPicker, FileService file)
    {
        this.folderPicker = folderPicker ?? throw new ArgumentNullException(nameof(folderPicker));
        this.file = file ?? throw new ArgumentNullException(nameof(file));

        BackCommand = new RelayCommand(_ => Back(), _ => IsBackCanExecute);
        NextCommand = new RelayCommand(_ => Next(), _ => IsNextCanExecute);
        PlayPauseCommand = new RelayCommand(_ => PlayPause(), _ => IsPlayPauseCanExecute);
        ZoomInCommand = new RelayCommand(_ => ZoomIn(), _ => selectedImageItem != null && ZoomFactor < SettingsManager.MaxZoom && !IsAutoPlay);
        ZoomOutCommand = new RelayCommand(_ => ZoomOut(), _ => selectedImageItem != null && ZoomFactor > SettingsManager.MinZoom && !IsAutoPlay);
        ResetZoomCommand = new RelayCommand(_ => ResetZoom(), _ => selectedImageItem != null && ZoomFactor != 1.0f && !IsAutoPlay);
        RotateCommand = new RelayCommand(_ => Rotate(), _ => selectedImageItem != null && !IsAutoPlay);
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
        ZoomFactor = Math.Min(ZoomFactor * SettingsManager.ZoomStep, SettingsManager.MaxZoom);
    }

    void ZoomOut()
    {
        ZoomFactor = Math.Max(ZoomFactor / SettingsManager.ZoomStep, SettingsManager.MinZoom);
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
        scale = Math.Clamp(scale, SettingsManager.MinZoom, SettingsManager.MaxZoom);

        ZoomFactor = (float)scale;
        ShouldFitToViewport = true;
    }
    #endregion
}