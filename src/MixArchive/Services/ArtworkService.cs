using Microsoft.Extensions.Options;
using MixArchive.Models;

namespace MixArchive.Services;

public class ArtworkService(IOptions<MusicOptions> options)
{
    private static readonly string[] AllowedExtensions = [".jpg", ".jpeg", ".png", ".webp"];

    public string GetFileName(int mixId, string extension)
    {
        if (string.IsNullOrWhiteSpace(extension))
            throw new InvalidOperationException("Missing artwork extension.");

        var ext = extension.ToLowerInvariant();

        if (!AllowedExtensions.Contains(ext))
        {
            throw new InvalidOperationException($"Unsupported artwork extension: {ext}");
        }

        return $"{mixId}{ext}";
    }

    public string GetFullPath(string fileName)
    {
        if (options.Value is null || string.IsNullOrWhiteSpace(options.Value.ArtworkPath))
            throw new InvalidOperationException("Music:ArtworkPath is not configured.");

        return Path.Combine(options.Value.ArtworkPath, fileName);
    }

    public bool Exists(string fileName) => File.Exists(GetFullPath(fileName));

    public void Delete(string fileName)
    {
        var path = GetFullPath(fileName);

        if (File.Exists(path))
            File.Delete(path);
    }
}
