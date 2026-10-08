using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MixArchive.Data;
using MixArchive.Services;

namespace MixArchive.Pages;

public sealed class IndexModel(MixArchiveDbContext db, MixScanner scanner) : PageModel
{
    public IList<Models.Mix> Mixes { get; private set; } = [];

    public int TotalMixes { get; private set; }

    [BindProperty(SupportsGet = true)]
    public string? Search { get; set; }

    [BindProperty]
    public ScanResult? LastScan { get; set; }

    public async Task OnGetAsync()
    {
        var query = db
            .Mixes.AsNoTracking()
            .Include(m => m.MixTags)
                .ThenInclude(mt => mt.Tag)
            .OrderBy(m => m.Title)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(Search))
        {
            var search = Search.Trim();

            query = query.Where(m =>
                EF.Functions.Like(m.Title, $"%{search}%")
                || EF.Functions.Like(m.Description, $"%{search}%")
                || m.MixTags.Any(mt => EF.Functions.Like(mt.Tag.Name, $"%{search}%"))
            );
        }

        Mixes = await query.ToListAsync();
        TotalMixes = await db.Mixes.CountAsync();
    }

    public async Task<IActionResult> OnPostResyncAsync()
    {
        LastScan = await scanner.ScanAsync();

        await OnGetAsync();

        return Page();
    }
}
