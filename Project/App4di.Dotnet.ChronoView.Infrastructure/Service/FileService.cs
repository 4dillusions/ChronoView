/*
4di .NET ChronoView application
Copyright (c) 2025 by 4D Illusions. All rights reserved.
Released under the terms of the GNU General Public License version 3 or later.
*/

using App4di.Dotnet.ChronoView.Infrastructure.DTO;

namespace App4di.Dotnet.ChronoView.Infrastructure.Service;

public class FileService
{
    public List<TimelineItemDTO> LoadImagesFromFolder(string folderPath, bool isAllFoldersRecursive, params string[] extensions)
    {
        if (extensions == null || extensions.Length == 0)
        {
            //extensions = [".jpg", ".jpeg"]; // Default extensions
            throw new ArgumentException("At least one file extension must be provided.", nameof(extensions));
        }

        var allowed = new HashSet<string>(extensions, StringComparer.OrdinalIgnoreCase);

        return Directory
            .EnumerateFiles(folderPath, "*.*", isAllFoldersRecursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly)
            .Where(path => allowed.Contains(Path.GetExtension(path)))
            .Select(path => new TimelineItemDTO
            {
                ImagePath = path,
                ImageName = Path.GetFileName(path),
                Timestamp = File.GetCreationTime(path)
            })
            .OrderBy(x => x.Timestamp)
            .ToList();
    }
}
