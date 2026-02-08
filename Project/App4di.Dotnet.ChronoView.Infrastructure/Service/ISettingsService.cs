/*
4di .NET ChronoView application
Copyright (c) 2025 by 4D Illusions. All rights reserved.
Released under the terms of the GNU General Public License version 3 or later.
*/

namespace App4di.Dotnet.ChronoView.Infrastructure.Service;

public interface ISettingsService
{
    #region Lifecycle
    void SaveSettings();
    #endregion

    #region App Settings
    string[] Languages { get; }
    string Language { get; set; }

    string[] Themes { get; }
    string Theme { get; set; }
    #endregion

    #region Window Settings
    int MinWidth { get; }
    int MinHeight { get; }
    bool IsTimelineCollapsed { get; set; }
    #endregion

    #region Image Settings
    float MinZoom { get; }
    float MaxZoom { get; }
    float ZoomStep { get; }

    string[] ImageFormats { get; }
    string ImageFormat { get; set; }
    bool IsRecursiveImageSearch { get; set; }
    #endregion
}
