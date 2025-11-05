/*
4di .NET ChronoView application
Copyright (c) 2025 by 4D Illusions. All rights reserved.
Released under the terms of the GNU General Public License version 3 or later.
*/

namespace App4di.Dotnet.ChronoView.Infrastructure.Helper;

public static class SettingsManager
{
    #region WindowSettings
    public const int MinWidth = 1280;
    public const int MinHeight = 768;
    #endregion

    #region Image settings
    public const float MinZoom = 0.5f;
    public const float MaxZoom = 8.0f;
    public const float ZoomStep = 1.25f;
    #endregion
}
