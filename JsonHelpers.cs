namespace BMCache;

public class MainFile
{
    public List<CachedFileInfo> Files { get; set; } = new List<CachedFileInfo>();
}

public class CachedFileInfo
{
    public DateTime Expired { get; set; }
    public string Identifier { get; set; }
}
