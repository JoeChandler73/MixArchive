using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using MixArchive.Data;
using MixArchive.Models;

namespace MixArchive.Pages.Mixes;

public class DetailsModel(MixArchiveDbContext db, IOptions<MusicOptions> options) : PageModel
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
        if (options.Value == null)
            throw new InvalidOperationException("Music:RootPath is not configured.");

        var rootPath = options.Value.RootPath;
        if (string.IsNullOrWhiteSpace(rootPath))
        {
            throw new InvalidOperationException("Music:RootPath is not configured.");
        }

        var mix = await db
            .Mixes.AsNoTracking()
            .Include(m => m.File)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (mix?.File == null)
            return NotFound();

        var filePath = Path.Combine(rootPath, mix.File.FileName);

        if (!System.IO.File.Exists(filePath))
            return NotFound();

        var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);

        return new FileStreamResult(stream, "audio/mpeg") { EnableRangeProcessing = true };
    }
}
