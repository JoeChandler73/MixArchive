namespace MixArchive.Models;

public class MixTag
{
    public int MixId { get; set; }

    public Mix Mix { get; set; } = null!;

    public int TagId { get; set; }

    public Tag Tag { get; set; } = null!;
}
