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
    #region Fields
    private float zoomFactor = 1.0f;

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
    #endregion

    #region Commands
    public ICommand ZoomInCommand { get; }
    public ICommand ZoomOutCommand { get; }
    public ICommand ResetZoomCommand { get; }
    #endregion

    #region CTor
    public HomeViewModel()
    {
        ZoomInCommand = new RelayCommand(
            _ => ZoomIn(),
            _ => ZoomFactor < SettingsManager.MaxZoom
        );

        ZoomOutCommand = new RelayCommand(
            _ => ZoomOut(),
            _ => ZoomFactor > SettingsManager.MinZoom
        );

        ResetZoomCommand = new RelayCommand(
            _ => ResetZoom(),
            _ => ZoomFactor != 1.0f
        );
    }
    #endregion

    #region Functions
    private void ZoomIn()
    {
        ZoomFactor = Math.Min(ZoomFactor * SettingsManager.ZoomStep, SettingsManager.MaxZoom);
    }

    private void ZoomOut()
    {
        ZoomFactor = Math.Max(ZoomFactor / SettingsManager.ZoomStep, SettingsManager.MinZoom);
    }

    private void ResetZoom()
    {
        ZoomFactor = 1.0f;
    }
    #endregion
}
