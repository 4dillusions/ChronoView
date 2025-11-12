/*
4di .NET ChronoView application
Copyright (c) 2025 by 4D Illusions. All rights reserved.
Released under the terms of the GNU General Public License version 3 or later.
*/

using App4di.Dotnet.ChronoView.WinUI.View;
using FW4di.Dotnet.Core.DependencyInjection;
using FW4di.Dotnet.MVVM.Service;

namespace App4di.Dotnet.ChronoView.WinUI.Service;

public class DIBindings
{
    IDIManager di { get; set; } = new DIManager();

    public void BindAllDepencies()
    {
        new Infrastructure.Service.DIBindings().BindAllDepencies(di,
            () =>
            {
                di.Bind<IFolderPickerService, FolderPickerService>(DILifetimeScopes.Singleton);
                di.Bind<INavigationService, NavigationService>(DILifetimeScopes.Singleton, new NavigationService(di));

                di.Bind<HomePage, HomePage>(DILifetimeScopes.Singleton);
                di.Bind<MainWindow, MainWindow>(DILifetimeScopes.Singleton);

            });
    }

    public T GetDependency<T>()
    {
        return di.GetDependency<T>();
    }
}
