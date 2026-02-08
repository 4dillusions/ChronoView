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
    private void InitSettings()
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
    public int MinWidth => settings.MinWidth;
    public int MinHeight => settings.MinHeight;

    public bool IsTimelineCollapsed
    {
        get => settings.IsTimelineCollapsed;
        set => settings.IsTimelineCollapsed = value;
    }
    #endregion

    #region Image Settings
    public float MinZoom => settings.MinZoom;
    public float MaxZoom => settings.MaxZoom;
    public float ZoomStep => settings.ZoomStep;

    public string[] ImageFormats => Enum.GetNames(typeof(ImageFormatType));

    public string ImageFormat
    {
        get => settings.imageFormat.ToString();
        set
        {
            if (Enum.TryParse<ImageFormatType>(value, out var format))
                settings.imageFormat = format;
        }
    }

    public bool IsRecursiveImageSearch
    {
        get => settings.IsRecursiveImageSearch;
        set => settings.IsRecursiveImageSearch = value;
    }
    #endregion
}
