using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using MixArchive.Data;
using MixArchive.Models;

namespace MixArchive.Services;

public class MixScanner(
    MixArchiveDbContext db,
    IOptions<MusicOptions> options,
    ILogger<MixScanner> logger
)
{
    public async Task<ScanResult> ScanAsync()
    {
        if (options.Value == null)
            throw new InvalidOperationException("Music:RootPath is not configured.");

        var rootPath = options.Value.RootPath;

        if (string.IsNullOrWhiteSpace(rootPath))
            throw new InvalidOperationException("Music:RootPath is not configured.");

        if (!Directory.Exists(rootPath))
            throw new DirectoryNotFoundException($"Music directory does not exist: {rootPath}");

        var result = new ScanResult();
        var files = Directory.EnumerateFiles(rootPath, "*.mp3", SearchOption.TopDirectoryOnly);

        foreach (var filePath in files)
        {
            result.FilesFound++;

            var fileName = Path.GetFileName(filePath);

            var existingFile = await db.MixFiles.FirstOrDefaultAsync(f => f.FileName == fileName);
            if (existingFile != null)
            {
                result.AlreadyKnown++;
                continue;
            }

            var mix = CreateMix(filePath);

            var mixFile = new MixFile
            {
                Mix = mix,
                FileName = fileName,
                FileSize = new FileInfo(filePath).Length,
                FileHash = await CalculateHashAsync(filePath),
                LastScanned = DateTime.UtcNow,
            };

            db.Mixes.Add(mix);
            db.MixFiles.Add(mixFile);

            result.NewMixes++;

            logger.LogInformation("Imported mix: {Title} ({FileName})", mix.Title, fileName);
        }

        await db.SaveChangesAsync();

        return result;
    }

    private static Mix CreateMix(string filePath)
    {
        using var file = TagLib.File.Create(filePath);

        var title = file.Tag.Title?.Trim();
        var artist = file.Tag.Performers?.FirstOrDefault()?.Trim();

        string mixTitle;

        if (!string.IsNullOrWhiteSpace(title) && !string.IsNullOrWhiteSpace(artist))
        {
            mixTitle = $"{artist} - {title}";
        }
        else if (!string.IsNullOrWhiteSpace(title))
        {
            mixTitle = title;
        }
        else if (!string.IsNullOrWhiteSpace(artist))
        {
            mixTitle = artist;
        }
        else
        {
            mixTitle = Path.GetFileNameWithoutExtension(filePath);
        }

        return new Mix
        {
            Title = mixTitle,
            Description = string.Empty,
            CreatedAt = DateTime.UtcNow,
            ModifiedAt = DateTime.UtcNow,
        };
    }

    private static async Task<string> CalculateHashAsync(string filePath)
    {
        await using var stream = File.OpenRead(filePath);

        var hash = await SHA256.HashDataAsync(stream);

        return Convert.ToHexString(hash);
    }
}

public class ScanResult
{
    public int FilesFound { get; set; }

    public int NewMixes { get; set; }

    public int AlreadyKnown { get; set; }
}
