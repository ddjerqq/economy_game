using Application.Services;
using Microsoft.Extensions.Caching.Memory;

#pragma warning disable CS1591
#pragma warning disable VSTHRD110
namespace Infrastructure.Services;

public sealed class IdempotencyService(IMemoryCache cache) : IIdempotencyService
{
    private static readonly TimeSpan InvalidationTime = TimeSpan.FromMinutes(10);

    public bool ContainsKey(Guid key) => cache.TryGetValue(key, out _);

    public void AddKey(Guid key)
    {
        var cacheOptions = new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = InvalidationTime,
        };

        cache.Set(key, key, cacheOptions);
    }
}