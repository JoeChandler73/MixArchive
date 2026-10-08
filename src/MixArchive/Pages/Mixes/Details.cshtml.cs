using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MixArchive.Data;
using MixArchive.Models;
using MixArchive.Services;

namespace MixArchive.Pages.Mixes;

public class DetailsModel(
    MixArchiveDbContext db,
    MixFileService fileService,
    ArtworkService artworkService
) : PageModel
{
    public Mix? Mix { get; private set; }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        Mix = await db
            .Mixes.AsNoTracking()
            .Include(m => m.File)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (Mix == null)
            return NotFound();

        return Page();
    }

    public async Task<IActionResult> OnGetAudioAsync(int id)
    {
        var mix = await db
            .Mixes.AsNoTracking()
            .Include(m => m.File)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (mix?.File == null)
            return NotFound();

        if (!fileService.Exists(mix.File.FileName))
            return NotFound();

        var stream = fileService.OpenRead(mix.File.FileName);

        return new FileStreamResult(stream, "audio/mpeg") { EnableRangeProcessing = true };
    }

    public async Task<IActionResult> OnGetArtworkAsync(int id)
    {
        var mix = await db.Mixes.AsNoTracking().FirstOrDefaultAsync(m => m.Id == id);

        if (mix?.ArtworkFileName == null)
            return NotFound();

        if (!artworkService.Exists(mix.ArtworkFileName))
            return NotFound();

        var path = artworkService.GetFullPath(mix.ArtworkFileName);

        var contentType = Path.GetExtension(path).ToLowerInvariant() switch
        {
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".webp" => "image/webp",
            _ => "application/octet-stream",
        };

        return PhysicalFile(path, contentType);
    }
}
