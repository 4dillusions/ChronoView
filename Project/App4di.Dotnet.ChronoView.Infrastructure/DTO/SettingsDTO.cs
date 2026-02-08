/*
4di .NET ChronoView application
Copyright (c) 2025 by 4D Illusions. All rights reserved.
Released under the terms of the GNU General Public License version 3 or later.
*/

namespace App4di.Dotnet.ChronoView.Infrastructure.DTO;

public class SettingsDTO
{
    #region AppSettings
    public ThemeType Theme { get; set; }
    public LanguageType Language { get; set; }
    #endregion

    #region WindowSettings
    public int MinWidth { get; set; }
    public int MinHeight { get; set; }
    public bool IsTimelineCollapsed { get; set; }
    #endregion

    #region Image settings
    public float MinZoom { get; set; }
    public float MaxZoom { get; set; }
    public float ZoomStep { get; set; }
    public ImageFormatType imageFormat { get; set; }
    public bool IsRecursiveImageSearch { get; set; }
    #endregion
}
