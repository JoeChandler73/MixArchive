namespace MixArchive.Models;

public sealed class MixFile
{
    public int Id { get; set; }

    public int MixId { get; set; }

    public Mix Mix { get; set; } = null!;

    public string FileName { get; set; } = string.Empty;

    public long FileSize { get; set; }

    public string FileHash { get; set; } = string.Empty;

    public DateTime LastScanned { get; set; }
}
