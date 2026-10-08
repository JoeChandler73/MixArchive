using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MixArchive.Data;
using MixArchive.Services;

namespace MixArchive.Pages.Mixes;

public class EditModel(MixArchiveDbContext db, TagService tagService, ArtworkService artworkService)
    : PageModel
{
    [BindProperty]
    public int Id { get; set; }

    [BindProperty]
    public string Title { get; set; } = string.Empty;

    [BindProperty]
    public string Description { get; set; } = string.Empty;

    [BindProperty]
    public string Tags { get; set; } = string.Empty;

    [BindProperty]
    public IFormFile? Artwork { get; set; }

    public string? ArtworkFileName { get; set; }

    [BindProperty]
    public bool RemoveArtwork { get; set; }

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
        ArtworkFileName = mix.ArtworkFileName;

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

        if (RemoveArtwork && !string.IsNullOrWhiteSpace(mix.ArtworkFileName))
        {
            artworkService.Delete(mix.ArtworkFileName);

            mix.ArtworkFileName = null;
        }

        if (Artwork is not null && Artwork.Length > 0)
        {
            if (Artwork.Length > 5 * 1024 * 1024)
            {
                ModelState.AddModelError(nameof(Artwork), "Artwork must be 5 MB or smaller.");

                return Page();
            }

            var extension = Path.GetExtension(Artwork.FileName).ToLowerInvariant();
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };

            if (!allowedExtensions.Contains(extension))
            {
                ModelState.AddModelError(nameof(Artwork), "Artwork must be JPEG, PNG or WebP.");

                return Page();
            }

            var fileName = artworkService.GetFileName(mix.Id, extension);
            var path = artworkService.GetFullPath(fileName);

            Directory.CreateDirectory(Path.GetDirectoryName(path)!);

            await using var stream = new FileStream(
                path,
                FileMode.Create,
                FileAccess.Write,
                FileShare.None
            );

            await Artwork.CopyToAsync(stream);

            mix.ArtworkFileName = fileName;
        }

        await db.SaveChangesAsync();

        return RedirectToPage("/Mixes/Details", new { id = mix.Id });
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
