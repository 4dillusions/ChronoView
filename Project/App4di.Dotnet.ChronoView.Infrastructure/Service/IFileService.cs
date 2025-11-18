/*
4di .NET ChronoView application
Copyright (c) 2025 by 4D Illusions. All rights reserved.
Released under the terms of the GNU General Public License version 3 or later.
*/

using App4di.Dotnet.ChronoView.Infrastructure.DTO;

namespace App4di.Dotnet.ChronoView.Infrastructure.Service;

public interface IFileService
{
    List<TimelineItemDTO> LoadImagesFromFolder(string folderPath, bool isAllFoldersRecursive, params string[] extensions);
}
