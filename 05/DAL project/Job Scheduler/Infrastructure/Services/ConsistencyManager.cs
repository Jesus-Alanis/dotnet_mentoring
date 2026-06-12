using Microsoft.Extensions.Caching.Memory;

namespace Infrastructure.Services;

public interface IConsistencyManager
{
    void TrackWrite(Guid userId);
    bool IsReadAfterWriteApplicable(Guid userId);
}

public class ConsistencyManager : IConsistencyManager
{
    private readonly IMemoryCache _cache;
    private readonly TimeSpan _cooldownPeriod = TimeSpan.FromSeconds(5);
    private const string CacheKeyPrefix = "UserLastWrite_";

    public ConsistencyManager(IMemoryCache cache)
    {
        _cache = cache;
    }

    /// <summary>
    /// Records that a write occurred for the user and starts the cooldown.
    /// </summary>
    public void TrackWrite(Guid userId)
    {
        var cacheKey = GetCacheKey(userId);
        _cache.Set(cacheKey, DateTime.UtcNow, _cooldownPeriod);
    }

    /// <summary>
    /// Determines if the user is within the cooldown period and needs a consistent read.
    /// </summary>
    public bool IsReadAfterWriteApplicable(Guid userId)
    {
        if (_cache.TryGetValue(GetCacheKey(userId), out DateTime lastWriteTime))
        {
            return (DateTime.UtcNow - lastWriteTime) < _cooldownPeriod;
        }
        return false;
    }

    private string GetCacheKey(Guid userId) => $"{CacheKeyPrefix}{userId}";
}
