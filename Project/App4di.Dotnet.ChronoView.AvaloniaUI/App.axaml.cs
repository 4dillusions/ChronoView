/*
4di .NET ChronoView application
Copyright (c) 2025 by 4D Illusions. All rights reserved.
Released under the terms of the GNU General Public License version 3 or later.
*/

using System;
using System.Globalization;
using App4di.Dotnet.ChronoView.AvaloniaUI.Helpers;
using App4di.Dotnet.ChronoView.Infrastructure.DTO;
using App4di.Dotnet.ChronoView.Infrastructure.Service;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using FW4di.Dotnet.Core.DependencyInjection;
using DIBindings = App4di.Dotnet.ChronoView.AvaloniaUI.Service.DIBindings;  

namespace App4di.Dotnet.ChronoView.AvaloniaUI;

public partial class App : Application
{
    DIBindings diBindings = new();
    ISettingsService settings;  
    
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        diBindings.BindAllDepencies();
        settings = diBindings.GetDependency<ISettingsService>(); 
        
        var cultureLanguageTag = Enum.Parse<LanguageType>(settings.Language).ToLanguageTag(); 
        var culture = new CultureInfo(cultureLanguageTag);
        CultureInfo.DefaultThreadCurrentCulture = culture;
        CultureInfo.DefaultThreadCurrentUICulture = culture;
        
        RequestedThemeVariant = ThemeVariantHelper.FromString(settings.Theme);
        
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow(diBindings.GetDependency<IDIManager>());
            
            desktop.MainWindow.Width = settings.MinWidth;
            desktop.MainWindow.Height = settings.MinHeight;
            
            desktop.MainWindow.Closed += (s, e) => settings.SaveSettings();
        }
        
        base.OnFrameworkInitializationCompleted();
    }
}