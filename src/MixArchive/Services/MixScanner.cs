using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using MixArchive.Data;
using MixArchive.Models;

namespace MixArchive.Services;

public class MixScanner(MixArchiveDbContext db, IOptions<MusicOptions> options)
{
    public async Task<ScanResult> ScanAsync()
    {
        if (options.Value is null || string.IsNullOrWhiteSpace(options.Value.RootPath))
        {
            throw new InvalidOperationException("Music:RootPath is not configured.");
        }

        var rootPath = options.Value.RootPath;

        if (!Directory.Exists(rootPath))
        {
            throw new DirectoryNotFoundException(rootPath);
        }

        var now = DateTime.UtcNow;

        var result = new ScanResult();

        var filesOnDisk = Directory
            .EnumerateFiles(rootPath, "*.mp3", SearchOption.TopDirectoryOnly)
            .ToList();

        result.FilesFound = filesOnDisk.Count;

        var existingFiles = await db.MixFiles.Include(f => f.Mix).ToListAsync();

        var byFileName = existingFiles.ToDictionary(
            f => f.FileName,
            StringComparer.OrdinalIgnoreCase
        );

        var byHash = existingFiles
            .Where(f => !string.IsNullOrWhiteSpace(f.FileHash))
            .GroupBy(f => f.FileHash)
            .ToDictionary(g => g.Key, g => g.First());

        var seenFiles = new HashSet<int>();

        foreach (var filePath in filesOnDisk)
        {
            var fileName = Path.GetFileName(filePath);

            if (byFileName.TryGetValue(fileName, out var existingFile))
            {
                existingFile.LastScanned = now;
                existingFile.LastSeen = now;

                seenFiles.Add(existingFile.Id);

                result.AlreadyKnown++;

                continue;
            }

            var hash = await CalculateHashAsync(filePath);

            if (byHash.TryGetValue(hash, out var renamedFile))
            {
                renamedFile.FileName = fileName;
                renamedFile.LastScanned = now;
                renamedFile.LastSeen = now;

                seenFiles.Add(renamedFile.Id);

                result.Renamed++;

                continue;
            }

            var mix = CreateMixFromFile(filePath);

            var mixFile = new MixFile
            {
                FileName = fileName,
                FileHash = hash,
                LastScanned = now,
                LastSeen = now,
                Mix = mix,
            };

            db.MixFiles.Add(mixFile);

            result.NewMixes++;
        }

        foreach (var file in existingFiles)
        {
            if (!seenFiles.Contains(file.Id) && file.LastSeen.HasValue)
            {
                result.Missing++;
            }
        }

        await db.SaveChangesAsync();

        return result;
    }

    private static Mix CreateMixFromFile(string filePath)
    {
        using var tagFile = TagLib.File.Create(filePath);

        var title = tagFile.Tag.Title;

        if (string.IsNullOrWhiteSpace(title))
        {
            title = Path.GetFileNameWithoutExtension(filePath);
        }

        var description = string.Empty;

        return new Mix
        {
            Title = title,
            Description = description,
            CreatedAt = DateTime.UtcNow,
            ModifiedAt = DateTime.UtcNow,
        };
    }

    private static async Task<string> CalculateHashAsync(string filePath)
    {
        await using var stream = File.OpenRead(filePath);

        using var sha256 = SHA256.Create();

        var hash = await sha256.ComputeHashAsync(stream);

        return Convert.ToHexString(hash);
    }
}
