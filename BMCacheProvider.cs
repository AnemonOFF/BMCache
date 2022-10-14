namespace BMCache;

public class BMCacheProvider
{
    private readonly List<BMCacher> _cachers;
    private readonly DirectoryInfo _directoryInfo;
    private readonly TimeSpan _expirationTime;

    public List<BMCacher> Cachers { get { return _cachers; } }

    public BMCacher this[string name] => _cachers.Find(x => x.Name == name) ?? throw new Exception($"{name} not exist.");

    /// <summary>
    /// Provider which can generate cachers. On init creating default cacher, which you can take by GetCacher("default") or indexing
    /// </summary>
    /// <param name="dir">Directory for cache files</param>
    /// <param name="defaultExpirationTime">Default cache expiration time. If null => 24 hours</param>
    public BMCacheProvider(string dir = "bmcache", TimeSpan? defaultExpirationTime = null)
    {
        _expirationTime = defaultExpirationTime ?? new TimeSpan(24, 0, 0);
        _directoryInfo = GetDirectory(dir);
        _cachers = new List<BMCacher>();
        CreateCacher("default");
    }

    /// <summary>
    /// Generate cacher
    /// </summary>
    /// <param name="name">Name of cacher</param>
    /// <param name="defaultExpirationTime">Default expiration time for this cacher. If null => provider default expiration time</param>
    /// <returns>Cacher instance</returns>
    public BMCacher CreateCacher(string name, TimeSpan? defaultExpirationTime = null)
    {
        var cacher = new BMCacher(name, _directoryInfo.FullName, defaultExpirationTime ?? _expirationTime);
        _cachers.Add(cacher);
        return cacher;
    }

    public BMCacher GetCacher(string name) => this[name];

    /// <summary>
    /// Remove cacher from provider. IT`S NOT DELETING FILES ON YOUR COMPUTER.
    /// </summary>
    /// <param name="name">Name of cacher</param>
    /// <exception cref="Exception"></exception>
    public void RemoveCacher(string name)
    {
        var cacher = _cachers.Find(x => x.Name == name);
        if (cacher == null)
            throw new Exception($"{name} not exist.");
        _cachers.Remove(cacher);
    }

    private static DirectoryInfo GetDirectory(string dir)
    {
        var dirInfo = new DirectoryInfo(dir);
        if (!dirInfo.Exists)
            dirInfo.Create();
        return dirInfo;
    }
}
