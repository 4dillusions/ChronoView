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
    #endregion

    #region Fields
    float zoomFactor = 1.0f;
    double currentRotationAngle = 0;
    double targetRotationAngle = 0;
    TimelineItemDTO? selectedImageItem;
    ObservableCollection<TimelineItemDTO> timelineItems = new();
    private int selectedIndex = -1;
    private bool isAutoPlay;

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
                //RaisePropertyChanged(nameof(HasSelectedImage));

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

    //public bool HasSelectedImage => SelectedImageItem != null;
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
        ZoomFactor = 1.0f;
    }

    void Rotate()
    {
        CurrentRotationAngle = TargetRotationAngle;
        TargetRotationAngle += 90;

        if (TargetRotationAngle >= 360)
            TargetRotationAngle = 0;
    }

    public async Task OpenFolder()
    {
        var path = await folderPicker.PickFolderAsync();
        if (path != null)
        {
            TimelineItems = new ObservableCollection<TimelineItemDTO>(file.LoadImagesFromFolder(path, ".jpg"));

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
    #endregion
}