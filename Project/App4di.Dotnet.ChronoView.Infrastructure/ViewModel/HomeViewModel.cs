/*
4di .NET ChronoView application
Copyright (c) 2025 by 4D Illusions. All rights reserved.
Released under the terms of the GNU General Public License version 3 or later.
*/

using App4di.Dotnet.ChronoView.Infrastructure.Helper;
using FW4di.Dotnet.MVVM;

namespace App4di.Dotnet.ChronoView.Infrastructure.ViewModel;

public class HomeViewModel : NotificationObject
{
    #region Commands
    public ICommand ZoomInCommand { get; }
    public ICommand ZoomOutCommand { get; }
    public ICommand ResetZoomCommand { get; }
    public ICommand RotateCommand { get; }
    #endregion

    #region Fields
    float zoomFactor = 1.0f;
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

    double currentRotationAngle = 0;
    public double CurrentRotationAngle
    {
        get => currentRotationAngle;
        set => SetProperty(ref currentRotationAngle, value);
    }

    double targetRotationAngle = 0;
    public double TargetRotationAngle
    {
        get => targetRotationAngle;
        set => SetProperty(ref targetRotationAngle, value);
    }
    #endregion

    #region CTor
    public HomeViewModel()
    {
        ZoomInCommand = new RelayCommand(_ => ZoomIn(), _ => ZoomFactor < SettingsManager.MaxZoom);
        ZoomOutCommand = new RelayCommand(_ => ZoomOut(), _ => ZoomFactor > SettingsManager.MinZoom);
        ResetZoomCommand = new RelayCommand(_ => ResetZoom(), _ => ZoomFactor != 1.0f);
        RotateCommand = new RelayCommand(_ => Rotate(), _ => true);
    }
    #endregion

    #region Functions
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
    #endregion
}
