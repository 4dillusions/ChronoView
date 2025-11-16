/*
4di .NET ChronoView application
Copyright (c) 2025 by 4D Illusions. All rights reserved.
Released under the terms of the GNU General Public License version 3 or later.
*/

using App4di.Dotnet.ChronoView.Infrastructure.DTO;
using FW4di.Dotnet.Core.IO;

namespace App4di.Dotnet.ChronoView.Infrastructure.Helper;

public static class SettingsManager
{
    static SettingsDTO settings;

    static void InitSettings()
    {
        settings = new SettingsDTO()
        {
            Theme = ThemeType.Dark,
            Language = LanguageType.EN,

            MinWidth = 1280,
            MinHeight = 768,
            IsTimelineCollapsed = false,

            MinZoom = 0.1f,
            MaxZoom = 8.0f,
            ZoomStep = 1.25f,
            imageFormat = ImageFormatType.jpg,
            IsRecursiveImageSearch = false,
        };
    }

    static SettingsManager()
    {
        var rootPath = AppContext.BaseDirectory;
        var configFolderPath = Path.Combine(rootPath, "Content");
        var configFileFullName = Path.Combine(configFolderPath, "Config.xml");

        if (!Directory.Exists(configFolderPath))
            Directory.CreateDirectory(configFolderPath);

        if (File.Exists(configFileFullName))
        {
            settings = XmlHelper.DeserializeFromFile<SettingsDTO>(configFileFullName)!;
        }
        else
        {
            InitSettings();
            XmlHelper.SerializeToFile(settings, configFileFullName);
        }
    }

    #region AppSettings
    public static string[] Languages => Enum.GetNames(typeof(LanguageType));
    public static string Language
    {
        get => settings.Language.ToString();
        
        set
        {
            if (Enum.TryParse<LanguageType>(value, out var lang))
            {
                settings.Language = lang;
            }
        }
    }

    public static string[] Themes => Enum.GetNames(typeof(ThemeType));
    public static string Theme
    {
        get => settings.Theme.ToString();
        
        set
        {
            if (Enum.TryParse<ThemeType>(value, out var theme))
            {
                settings.Theme = theme;
            }
        }
    }
    #endregion

    #region WindowSettings
    public static int MinWidth
    {
        get => settings.MinWidth;
    }

    public static int MinHeight
    {
        get => settings.MinHeight;
    }

    public static bool IsTimelineCollapsed
    {
        get => settings.IsTimelineCollapsed;
        set => settings.IsTimelineCollapsed = value;
    }
    #endregion

    #region Image settings
    public static float MinZoom 
    { 
        get => settings.MinZoom;
    }

    public static float MaxZoom
    {         
        get => settings.MaxZoom;
    }

    public static float ZoomStep
    {
        get => settings.ZoomStep;
    }

    public static string[] ImageFormats => Enum.GetNames(typeof(ImageFormatType));
    public static string ImageFormat
    {
        get => settings.imageFormat.ToString();

        set
        {
            if (Enum.TryParse<ImageFormatType>(value, out var format))
            {
                settings.imageFormat = format;
            }
        }
    }

    public static bool IsRecursiveImageSearch
    {
        get => settings.IsRecursiveImageSearch;
        set => settings.IsRecursiveImageSearch = value;
    }
    #endregion
}
