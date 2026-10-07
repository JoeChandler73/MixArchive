using Microsoft.Extensions.Options;
using MixArchive.Models;

namespace MixArchive.Services;

public class MixFileService(IOptions<MusicOptions> options)
{
    public string GetFullPath(string fileName)
    {
        if (options.Value == null || string.IsNullOrWhiteSpace(options.Value.RootPath))
        {
            throw new InvalidOperationException("Music:RootPath is not configured.");
        }

        return Path.Combine(options.Value.RootPath, fileName);
    }

    public bool Exists(string fileName) => File.Exists(GetFullPath(fileName));

    public FileStream OpenRead(string fileName)
    {
        var path = GetFullPath(fileName);

        return new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
    }
}
