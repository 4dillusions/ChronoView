/*
4di .NET ChronoView application
Copyright (c) 2025 by 4D Illusions. All rights reserved.
Released under the terms of the GNU General Public License version 3 or later.
*/

using System;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;
using FW4di.Dotnet.MVVM.Service;

namespace App4di.Dotnet.ChronoView.AvaloniaUI.Service;

public sealed class FolderPickerService : IFolderPickerService
{
    public async Task<string?> PickFolderAsync()
    {
        if (Application.Current?.ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime desktop)
            return null;

        var window = desktop.MainWindow;
        if (window?.StorageProvider is null)
            return null;

        var folders = await window.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
        {
            AllowMultiple = false,
            Title = "Select folder"
        });

        var folder = folders.FirstOrDefault();
        return folder?.Path.LocalPath;
    }
}
