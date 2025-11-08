/*
4di .NET ChronoView application
Copyright (c) 2025 by 4D Illusions. All rights reserved.
Released under the terms of the GNU General Public License version 3 or later.
*/

using App4di.Dotnet.ChronoView.Infrastructure.Service;
using System;
using System.Threading.Tasks;
using WinRT.Interop;

namespace App4di.Dotnet.ChronoView.WinUI.Service;

public sealed class FolderPickerService : IFolderPickerService
{
    public async Task<string?> PickFolderAsync()
    {
        var picker = new Windows.Storage.Pickers.FolderPicker();
        picker.FileTypeFilter.Add("*");

        ArgumentNullException.ThrowIfNull(App.MainWindow);
        var hwnd = WindowNative.GetWindowHandle(App.MainWindow);
        InitializeWithWindow.Initialize(picker, hwnd);

        var folder = await picker.PickSingleFolderAsync();
        return folder?.Path;
    }
}
