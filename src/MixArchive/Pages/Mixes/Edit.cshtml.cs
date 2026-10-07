using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MixArchive.Data;
using MixArchive.Services;

namespace MixArchive.Pages.Mixes;

public class EditModel(MixArchiveDbContext db, TagService tagService) : PageModel
{
    [BindProperty]
    public int Id { get; set; }

    [BindProperty]
    public string Title { get; set; } = string.Empty;

    [BindProperty]
    public string Description { get; set; } = string.Empty;

    [BindProperty]
    public string Tags { get; set; } = string.Empty;

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var mix = await db
            .Mixes.AsNoTracking()
            .Include(m => m.MixTags)
                .ThenInclude(mt => mt.Tag)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (mix == null)
            return NotFound();

        Id = mix.Id;
        Title = mix.Title;
        Description = mix.Description;

        Tags = string.Join(", ", mix.MixTags.Select(mt => mt.Tag.Name).OrderBy(name => name));

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var mix = await db.Mixes.Include(m => m.MixTags).FirstOrDefaultAsync(m => m.Id == Id);

        if (mix == null)
            return NotFound();

        if (string.IsNullOrWhiteSpace(Title))
        {
            ModelState.AddModelError(nameof(Title), "Title is required.");
        }

        if (!ModelState.IsValid)
            return Page();

        mix.Title = Title.Trim();
        mix.Description = Description.Trim();
        mix.ModifiedAt = DateTime.UtcNow;

        var tagNames = Tags.Split(
            [','],
            StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries
        );

        var tags = await tagService.GetOrCreateTagsAsync(tagNames);

        mix.MixTags.Clear();

        foreach (var tag in tags)
        {
            mix.MixTags.Add(new Models.MixTag { MixId = mix.Id, TagId = tag.Id });
        }

        await db.SaveChangesAsync();

        return RedirectToPage("/Mixes/Details", new { id = mix.Id });
    }
}
