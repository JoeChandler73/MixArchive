namespace MixArchive.Models;

public sealed class Mix
{
    public int Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }

    public DateTime ModifiedAt { get; set; }

    public MixFile? File { get; set; }
}
