using App4di.Dotnet.ChronoView.Infrastructure.Service;
using FW4di.Dotnet.MVVM;

namespace App4di.Dotnet.ChronoView.Infrastructure.ViewModel;

public class SettingsViewModel : NotificationObject
{
    #region Fields
    private ISettingsService settings;

    // ---- APP ----
    public string[] Themes => settings.Themes;
    public string Theme
    {
        get => settings.Theme;
        set
        {
            if (settings.Theme == value) return;
            settings.Theme = value;
            RaisePropertyChanged(nameof(Theme));
        }
    }

    public string[] Languages => settings.Languages;
    public string Language
    {
        get => settings.Language;
        set
        {
            if (settings.Language == value) return;
            settings.Language = value;
            RaisePropertyChanged(nameof(Language));
        }
    }

    // ---- WINDOW ----
    public int MinWidth
    {
        get => settings.MinWidth;
        set
        {
            if (settings.MinWidth == value) return;
            settings.MinWidth = value;
            RaisePropertyChanged(nameof(MinWidth));
        }
    }

    public int MinHeight
    {
        get => settings.MinHeight;
        set
        {
            if (settings.MinHeight == value) return;
            settings.MinHeight = value;
            RaisePropertyChanged(nameof(MinHeight));
        }
    }

    public bool IsTimelineCollapsed
    {
        get => settings.IsTimelineCollapsed;
        set
        {
            if (settings.IsTimelineCollapsed == value) return;
            settings.IsTimelineCollapsed = value;
            RaisePropertyChanged(nameof(IsTimelineCollapsed));
        }
    }

    // ---- IMAGE ----
    public int MinZoom
    {
        get => (int)(settings.MinZoom * 100);
        set
        {
            settings.MinZoom = value / 100.0f;
            RaisePropertyChanged(nameof(MinZoom));
        }
    }

    public int MaxZoom
    {
        get => (int)(settings.MaxZoom * 100);
        set
        {
            settings.MaxZoom = value / 100.0f; ;
            RaisePropertyChanged(nameof(MaxZoom));
        }
    }

    public int ZoomStep
    {
        get => (int)(settings.ZoomStep * 100);
        set
        {
            settings.ZoomStep = value / 100.0f; ;
            RaisePropertyChanged(nameof(ZoomStep));
        }
    }

    public string[] ImageFormats => settings.ImageFormats;
    public string ImageFormat
    {
        get => settings.ImageFormat;
        set
        {
            if (settings.ImageFormat == value) return;
            settings.ImageFormat = value;
            RaisePropertyChanged(nameof(ImageFormat));
        }
    }

    public bool IsRecursiveImageSearch
    {
        get => settings.IsRecursiveImageSearch;
        set
        {
            if (settings.IsRecursiveImageSearch == value) return;
            settings.IsRecursiveImageSearch = value;
            RaisePropertyChanged(nameof(IsRecursiveImageSearch));
        }
    }
    #endregion

    #region CTor
    public SettingsViewModel(ISettingsService settings)
    {
        this.settings = settings ?? throw new ArgumentNullException(nameof(settings));

        ResetDefaultCommand = new RelayCommand(_ => ResetDefault());
    }
    #endregion

    #region Functions
    void RefreshAll()
    {
        RaisePropertyChanged(nameof(Themes));
        RaisePropertyChanged(nameof(Theme));
        RaisePropertyChanged(nameof(Languages));
        RaisePropertyChanged(nameof(Language));

        RaisePropertyChanged(nameof(MinWidth));
        RaisePropertyChanged(nameof(MinHeight));
        RaisePropertyChanged(nameof(IsTimelineCollapsed));

        RaisePropertyChanged(nameof(MinZoom));
        RaisePropertyChanged(nameof(MaxZoom));
        RaisePropertyChanged(nameof(ZoomStep));
        RaisePropertyChanged(nameof(ImageFormats));
        RaisePropertyChanged(nameof(ImageFormat));
        RaisePropertyChanged(nameof(IsRecursiveImageSearch));
    }

    void ResetDefault()
    {
        settings.InitSettings();
        settings.SaveSettings();
        RefreshAll();
    }
    #endregion

    #region Commands
    public ICommand ResetDefaultCommand { get; }
    #endregion
}
