<p align="center">
<img src="https://raw.githubusercontent.com/AnemonOFF/BMCache/main/logo.png" alt="BMCache">
<h1 align="center">BMCache</h1>
</p>
<p align="center">
<a href="https://www.nuget.org/packages/BMCache/"><img alt="Nuget" src="https://img.shields.io/nuget/v/BMCache"></a>
<a href="https://github.com/AnemonOFF/BMCache/blob/main/LICENSE"><img alt="GitHub" src="https://img.shields.io/github/license/AnemonOFF/BMCache"></a>
</p>
</br>

A simple C# cacher. Caching in file.

## How to use

 1. Firstly **create cache provider**
 ```csharp
 var provider = new BMCacheProvider();
 ```
 | Agrument              | type      | required | default  | description                                   |
|-----------------------|-----------|----------|----------|-----------------------------------------------|
| dir                   | string    | -        | bmcache  | directory where storage cache data            |
| defaultExpirationTime | TimeStamp | -        | 24 hours | Default expiration time to delete cached file |
 2. Create cacher by provider. **DO NOT** create it by yourself (using new BMCacher()).
 ```csharp
 var cacher = provider.CreateCacher("test");
 ```
 | Agrument              | type      | required | default                          | description                                            |
|-----------------------|-----------|----------|----------------------------------|--------------------------------------------------------|
| name                  | string    | +        |                                  | directory (and cacher) name, where to save cached data |
| defaultExpirationTime | TimeStamp | -        | Provider`s defaultExpirationTime | Default expiration time to delete cached file          |
 3. **Cache**
 ```csharp
// You can cache and get it back via diferrent methods
var variableToCache = "cache me";
cacher.Cache("identifier", variableToCache);
var variableFromCache = cacher.Get<string>("identifier");
// You can use default methods with just value argument.
// This methods will give you cached before value
// or cache it and throw back
var cachedTime = cacher.GetOrCache<DateTime>("dt", DateTime.UtcNow);
var cachedDoub = await cacher.GetOrCacheAsync<double>("double", 123.456);
// Or use value generator
Func<int> intFunction = () => 12345;
var cachedNum = cacher.GetOrCache<int>("num", intFunction);
var cachedNumAsync = await cacher.GetOrCacheAsync<int>("num", intFunction);
// Or async value generator
Func<Task<List<string>>> listAsyncFunc = async () => new List<string>() { "first", "second", "third" };
var cachedList = await cacher.GetOrCacheAsync<List<string>>("list", listAsyncFunc);
```
In addition you can set expiration time for each caching separately
```csharp
// In example, this row will cache string for 2 hour from now
cacher.Cache("identifier", "string to cache", new TimeStamp(2, 0, 0));
```
***For expiration control, library using UTC time**
## License
[License](https://github.com/AnemonOFF/BMCache/blob/master/LICENSE)
