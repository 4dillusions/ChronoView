/*
4di .NET ChronoView application
Copyright (c) 2025 by 4D Illusions. All rights reserved.
Released under the terms of the GNU General Public License version 3 or later.
*/

using FW4di.Dotnet.MVVM.Service;
using Microsoft.UI.Xaml;
using System;
using System.Threading.Tasks;
using WinRT.Interop;

namespace App4di.Dotnet.ChronoView.WinUI.Service;

public sealed class FolderPickerService(MainWindow mainWindow) : IFolderPickerService
{
    Window mainWindow = mainWindow ?? throw new ArgumentNullException(nameof(mainWindow));

    public async Task<string?> PickFolderAsync()
    {
        var picker = new Windows.Storage.Pickers.FolderPicker();
        picker.FileTypeFilter.Add("*");

        ArgumentNullException.ThrowIfNull(mainWindow);
        var hwnd = WindowNative.GetWindowHandle(mainWindow);
        InitializeWithWindow.Initialize(picker, hwnd);

        var folder = await picker.PickSingleFolderAsync();
        return folder?.Path;
    }
}
