using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MixArchive.Data;

namespace MixArchive.Pages;

public sealed class IndexModel(MixArchiveDbContext db) : PageModel
{
    public IList<Models.Mix> Mixes { get; private set; } = [];

    public int TotalMixes { get; private set; }

    [BindProperty(SupportsGet = true)]
    public string? Search { get; set; }

    public async Task OnGetAsync()
    {
        var query = db.Mixes.AsNoTracking().OrderBy(m => m.Title).AsQueryable();

        if (!string.IsNullOrWhiteSpace(Search))
        {
            var search = Search.Trim();

            query = query.Where(m =>
                EF.Functions.Like(m.Title, $"%{search}%")
                || EF.Functions.Like(m.Description, $"%{search}%")
            );
        }

        Mixes = await query.ToListAsync();
        TotalMixes = await db.Mixes.CountAsync();
    }
}
