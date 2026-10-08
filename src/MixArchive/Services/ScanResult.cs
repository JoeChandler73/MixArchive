namespace MixArchive.Services;

public class ScanResult
{
    public int FilesFound { get; set; }

    public int NewMixes { get; set; }

    public int AlreadyKnown { get; set; }

    public int Renamed { get; set; }

    public int Missing { get; set; }
}
