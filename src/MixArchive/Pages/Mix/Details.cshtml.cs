using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MixArchive.Data;

namespace MixArchive.Pages.Mix;

public class DetailsModel(MixArchiveDbContext db, IConfiguration configuration) : PageModel
{
    public Models.Mix? Mix { get; private set; }

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

        var rootPath = configuration["Music:RootPath"];

        if (string.IsNullOrWhiteSpace(rootPath))
        {
            throw new InvalidOperationException("Music:RootPath is not configured.");
        }

        var filePath = Path.Combine(rootPath, mix.File.FileName);

        if (!System.IO.File.Exists(filePath))
            return NotFound();

        var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);

        return new FileStreamResult(stream, "audio/mpeg") { EnableRangeProcessing = true };
    }
}
