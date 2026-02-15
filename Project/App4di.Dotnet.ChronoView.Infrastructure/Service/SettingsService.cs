/*
4di .NET ChronoView application
Copyright (c) 2025 by 4D Illusions. All rights reserved.
Released under the terms of the GNU General Public License version 3 or later.
*/

using App4di.Dotnet.ChronoView.Infrastructure.DTO;
using FW4di.Dotnet.Core.IO;

namespace App4di.Dotnet.ChronoView.Infrastructure.Service;

public class SettingsService : ISettingsService
{
    #region Fields
    private SettingsDTO settings;
    private string configFileFullName;
    #endregion

    #region CTor
    public SettingsService()
    {
        CreateService();
    }
    #endregion

    #region Functions
    public void InitSettings()
    {
        settings = new SettingsDTO()
        {
            Theme = ThemeType.Dark,
            Language = LanguageType.EN,

            MinWidth = 1280,
            MinHeight = 768,
            IsTimelineCollapsed = false,

            MinZoom = 4.31f,
            MaxZoom = 8.6f,
            ZoomStep = 1.37f,
            ImageFormat = ImageFormatType.jpg,
            IsRecursiveImageSearch = false,
        };
    }

    public void SaveSettings()
    {
        XmlHelper.SerializeToFile(settings, configFileFullName);
    }

    void CreateService()
    {
        var rootPath = AppContext.BaseDirectory;
        var configFolderPath = Path.Combine(rootPath, "Content");
        configFileFullName = Path.Combine(configFolderPath, "Config.xml");

        if (!Directory.Exists(configFolderPath))
            Directory.CreateDirectory(configFolderPath);

        if (File.Exists(configFileFullName))
        {
            settings = XmlHelper.DeserializeFromFile<SettingsDTO>(configFileFullName)!;
        }
        else
        {
            InitSettings();
            SaveSettings();
        }
    }
    #endregion

    #region App Settings
    public string[] Languages => Enum.GetNames(typeof(LanguageType));

    public string Language
    {
        get => settings.Language.ToString();
        set
        {
            if (Enum.TryParse<LanguageType>(value, out var lang))
                settings.Language = lang;
        }
    }

    public string[] Themes => Enum.GetNames(typeof(ThemeType));

    public string Theme
    {
        get => settings.Theme.ToString();
        set
        {
            if (Enum.TryParse<ThemeType>(value, out var theme))
                settings.Theme = theme;
        }
    }
    #endregion

    #region Window Settings
    public int MinWidth
    {
        get => settings.MinWidth;
        set => settings.MinWidth = value;
    }

    public int MinHeight
    {
        get => settings.MinHeight;
        set => settings.MinHeight = value;
    }

    public bool IsTimelineCollapsed
    {
        get => settings.IsTimelineCollapsed;
        set => settings.IsTimelineCollapsed = value;
    }
    #endregion

    #region Image Settings
    public float MinZoom
    {
        get => settings.MinZoom;
        set => settings.MinZoom = value;
    }

    public float MaxZoom
    {
        get => settings.MaxZoom;
        set => settings.MaxZoom = value;
    }

    public float ZoomStep
    {
        get => settings.ZoomStep;
        set => settings.ZoomStep = value;
    }
    
    public string[] ImageFormats => Enum.GetNames(typeof(ImageFormatType));

    public string ImageFormat
    {
        get => settings.ImageFormat.ToString();
        set
        {
            if (Enum.TryParse<ImageFormatType>(value, out var format))
                settings.ImageFormat = format;
        }
    }

    public bool IsRecursiveImageSearch
    {
        get => settings.IsRecursiveImageSearch;
        set => settings.IsRecursiveImageSearch = value;
    }
    #endregion
}
