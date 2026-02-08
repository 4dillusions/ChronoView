/*
4di .NET ChronoView application
Copyright (c) 2025 by 4D Illusions. All rights reserved.
Released under the terms of the GNU General Public License version 3 or later.
*/

using App4di.Dotnet.ChronoView.Infrastructure.ViewModel;
using FW4di.Dotnet.Core.DependencyInjection;

namespace App4di.Dotnet.ChronoView.Infrastructure.Service;

public class DIBindings
{
    public void BindAllDepencies(IDIManager di, Action bindings)
    {
        di.Init
        (
            () =>
            {
                di.Bind<IFileService, FileService>(DILifetimeScopes.Singleton);
                di.Bind<ISettingsService, SettingsService>(DILifetimeScopes.Singleton);

                di.Bind<HomeViewModel, HomeViewModel>(DILifetimeScopes.Transient);
                di.Bind<TimelineViewModel, TimelineViewModel>(DILifetimeScopes.Transient);

                bindings();
            }
        );
    }
}
