using Microsoft.EntityFrameworkCore;
using MixArchive.Data;
using MixArchive.Models;

namespace MixArchive.Services;

public class TagService(MixArchiveDbContext db)
{
    public async Task<List<Tag>> GetOrCreateTagsAsync(IEnumerable<string> tagNames)
    {
        var names = tagNames
            .Select(Normalise)
            .Where(name => !string.IsNullOrWhiteSpace(name))
            .Distinct()
            .ToList();

        if (names.Count == 0)
            return [];

        var existingTags = await db.Tags.Where(t => names.Contains(t.Name)).ToListAsync();

        var existingNames = existingTags.Select(t => t.Name).ToHashSet();

        var newTags = names
            .Where(name => !existingNames.Contains(name))
            .Select(name => new Tag { Name = name })
            .ToList();

        if (newTags.Count > 0)
        {
            db.Tags.AddRange(newTags);
            await db.SaveChangesAsync();
        }

        existingTags.AddRange(newTags);

        return existingTags;
    }

    private static string Normalise(string value) => value.Trim().ToLowerInvariant();
}
