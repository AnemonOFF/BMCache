using Newtonsoft.Json;

namespace BMCache;

public class BMCacher
{
    private readonly string _name;
    private readonly string _dir;
    private readonly TimeSpan _expirationTime;
    private readonly MainFile _mainFile;

    public string Name { get { return _name; } }

    /// <summary>
    /// Cacher object. CREATE IT ONLY FROM BMCacheProvider
    /// </summary>
    /// <param name="name">Name of cacher</param>
    /// <param name="dir">Caching directory</param>
    /// <param name="defaultExpirationTime">Default expiration time</param>
    public BMCacher(string name, string dir, TimeSpan defaultExpirationTime)
    {
        _name = name;
        _dir = dir;
        _expirationTime = defaultExpirationTime;
        _mainFile = GetMainFile(_dir, _name);
        Init(_mainFile, _dir, _name);
    }

    /// <summary>
    /// Async implementation of GetOrCache method. Caching object using generator function or return previous cached instance.
    /// </summary>
    /// <typeparam name="T">The type of object being caching</typeparam>
    /// <param name="identifier">Unique ID</param>
    /// <param name="generator">Generator function to get caching object</param>
    /// <param name="expirationTime">Expiration time, if null set default expiration which set in provider</param>
    /// <returns>New instance of cached object</returns>
    public async Task<T> GetOrCacheAsync<T>(string identifier, Func<Task<T>> generator, TimeSpan? expirationTime = null)
    {
        if(Contains(identifier))
            return (await GetAsync<T>(identifier))!;
        var cachingObject = await generator();
        await CacheAsync(identifier, cachingObject, expirationTime);
        return cachingObject;
    }

    /// <summary>
    /// Async implementation of GetOrCache method. Caching object using generator function or return previous cached instance.
    /// </summary>
    /// <typeparam name="T">The type of object being caching</typeparam>
    /// <param name="identifier">Unique ID</param>
    /// <param name="generator">Generator function to get caching object</param>
    /// <param name="expirationTime">Expiration time, if null set default expiration which set in provider</param>
    /// <returns>New instance of cached object</returns>
    public async Task<T> GetOrCacheAsync<T>(string identifier, Func<T> generator, TimeSpan? expirationTime = null)
        => await GetOrCacheAsync(identifier, async () => await Task.Run(generator), expirationTime);

    /// <summary>
    /// Async implementation of GetOrCache method.  Caching object instance or return previous cached instance.
    /// </summary>
    /// <typeparam name="T">The type of object being caching</typeparam>
    /// <param name="identifier">Unique ID</param>
    /// <param name="cacheableObject">The object instance to cache</param>
    /// <param name="expirationTime">Expiration time, if null set default expiration which set in provider</param>
    /// <returns>New instance of cached object</returns>
    public async Task<T> GetOrCacheAsync<T>(string identifier, T cacheableObject, TimeSpan? expirationTime = null)
        => await GetOrCacheAsync(identifier, async () => await Task.Run(() => cacheableObject), expirationTime);

    /// <summary>
    /// Caching object using generator function or return previous cached instance.
    /// </summary>
    /// <typeparam name="T">The type of object being caching</typeparam>
    /// <param name="identifier">Unique ID</param>
    /// <param name="generator">Generator function to get caching object</param>
    /// <param name="expirationTime">Expiration time, if null set default expiration which set in provider</param>
    /// <returns>New instance of cached object</returns>
    public T GetOrCache<T>(string identifier, Func<T> generator, TimeSpan? expirationTime = null)
    {
        if (Contains(identifier))
            return Get<T>(identifier)!;
        var cachingObject = generator();
        Cache(identifier, cachingObject, expirationTime);
        return cachingObject;
    }

    /// <summary>
    /// Caching object instance or return previous cached instance.
    /// </summary>
    /// <typeparam name="T">The type of object being caching</typeparam>
    /// <param name="identifier">Unique ID</param>
    /// <param name="cacheableObject">The object instance to cache</param>
    /// <param name="expirationTime">Expiration time, if null set default expiration which set in provider</param>
    /// <returns>New instance of cached object</returns>
    public T GetOrCache<T>(string identifier, T cacheableObject, TimeSpan? expirationTime = null)
        => GetOrCache(identifier, () => cacheableObject, expirationTime);

    /// <summary>
    /// Is any cached item with identifier
    /// </summary>
    /// <param name="identifier">Unique ID</param>
    /// <returns>Contains or not</returns>
    public bool Contains(string identifier)
        => _mainFile.Files.Any(x => x.Identifier == identifier);

    /// <summary>
    /// Caching the given object instance.
    /// </summary>
    /// <typeparam name="T">The type of object being caching</typeparam>
    /// <param name="identifier">Unique ID</param>
    /// <param name="cacheableObject">The object instance to cache</param>
    /// <param name="expirationTime">Expiration time, if null set default expiration which set in provider</param>
    public void Cache<T>(string identifier, T cacheableObject, TimeSpan? expirationTime = null)
    {
        var content = JsonConvert.SerializeObject(cacheableObject);
        var path = Path.Combine(_dir, _name, identifier + ".bm");
        using var writer = new StreamWriter(path, false);
        writer.Write(content);
        writer.Flush();
        writer.Close();
        AddCachedFileInfo(identifier, expirationTime);
    }

    /// <summary>
    /// Async implementation of Cache method. Caching the given object instance.
    /// </summary>
    /// <typeparam name="T">The type of object being caching</typeparam>
    /// <param name="identifier">Unique ID</param>
    /// <param name="cacheableObject">The object instance to cache</param>
    /// <param name="expirationTime">Expiration time, if null set default expiration which set in provider</param>
    /// <returns></returns>
    public async Task CacheAsync<T>(string identifier, T cacheableObject, TimeSpan? expirationTime = null)
        => await Task.Run(() => Cache(identifier, cacheableObject, expirationTime));

    /// <summary>
    /// Read an object instance from cache.
    /// </summary>
    /// <typeparam name="T">The type of object to read from cache</typeparam>
    /// <param name="identifier">Unique ID</param>
    /// <returns>Returns new instance of the object read from cache</returns>
    public T? Get<T>(string identifier)
    {
        var info = _mainFile.Files.Find(x => x.Identifier == identifier);
        if (info == null)
            return default;
        if(info.Expired < DateTime.UtcNow)
        {
            DeleteCachedFile(identifier);
            return default;
        }
        var path = Path.Combine(_dir, _name, identifier + ".bm");
        var fileInfo = new FileInfo(path);
        if(!fileInfo.Exists)
            return default;
        using var reader = new StreamReader(path);
        var content = reader.ReadToEnd();
        return JsonConvert.DeserializeObject<T>(content);
    }

    /// <summary>
    /// Async implementation of Read method. Read an object instance from cache.
    /// </summary>
    /// <typeparam name="T">The type of object to read from cache</typeparam>
    /// <param name="identifier">Unique ID</param>
    /// <returns>Returns Task with new instance of the object read from cache</returns>
    public async Task<T?> GetAsync<T>(string identifier)
        => await Task.Run(() => Get<T>(identifier));

    private void Init(MainFile mainFile, string dir, string name)
    {
        foreach (var info in new List<CachedFileInfo>(mainFile.Files))
        {
            var fileInfo = new FileInfo(Path.Combine(dir, name, info.Identifier + ".bm"));
            if (info.Expired < DateTime.UtcNow || !fileInfo.Exists)
            {
                DeleteCachedFile(info.Identifier);
                if (fileInfo.Exists)
                    fileInfo.Delete();
            }
        }
    }

    private void DeleteCachedFile(string identifier)
    {
        var info = _mainFile.Files.Find(x => x.Identifier == identifier);
        if (info == null)
            throw new Exception($"{identifier} not exist.");
        _mainFile.Files.Remove(info);
        Save();
        var fileInfo = new FileInfo(Path.Combine(_dir, _name, identifier + ".bm"));
        if (fileInfo.Exists)
            fileInfo.Delete();
    }

    private void AddCachedFileInfo(string identifier, TimeSpan? expirationTime = null)
    {
        var info = new CachedFileInfo
        {
            Expired = DateTime.UtcNow.AddTicks(expirationTime?.Ticks ?? _expirationTime.Ticks),
            Identifier = identifier
        };
        _mainFile.Files.Add(info);
        Save();
    }

    private void Save()
    {
        var path = Path.Combine(_dir, _name, "main.json");
        using var writer = new StreamWriter(path, false);
        writer.Write(JsonConvert.SerializeObject(_mainFile));
        writer.Flush();
        writer.Close();
    }

    private static MainFile GetMainFile(string dir, string name)
    {
        var path = Path.Combine(dir, name, "main.json");
        var dirInfo = new DirectoryInfo(Path.Combine(dir, name));
        if (!dirInfo.Exists)
            dirInfo.Create();
        MainFile? result;
        using (var readStream = File.Open(path, FileMode.OpenOrCreate))
        {
            using var reader = new StreamReader(readStream);
            var content = reader.ReadToEnd();
            result = JsonConvert.DeserializeObject<MainFile>(content);
        }
        return result ?? new MainFile();
    }
}
