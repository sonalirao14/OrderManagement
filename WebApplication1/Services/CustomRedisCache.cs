using Microsoft.Extensions.Caching.Distributed;
using StackExchange.Redis;

namespace WebApplication1.Services

{
    public class CustomRedisCache: IDistributedCache
    {
        private readonly IConnectionMultiplexer _redis;
        private readonly int _database;

        public CustomRedisCache(IConnectionMultiplexer redis)
        {
            _redis = redis ?? throw new ArgumentNullException(nameof(redis));
            _database = 1; 
        }

        public byte[] Get(string key)
        {
            return GetAsync(key).GetAwaiter().GetResult();
        }

        public async Task<byte[]> GetAsync(string key, CancellationToken token = default)
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException(nameof(key));

            var db = _redis.GetDatabase(_database);
            return await db.StringGetAsync(key);
        }

        public void Set(string key, byte[] value, DistributedCacheEntryOptions options)
        {
            SetAsync(key, value, options).GetAwaiter().GetResult();
        }

        public async Task SetAsync(string key, byte[] value, DistributedCacheEntryOptions options, CancellationToken token = default)
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException(nameof(key));
            if (value == null)
                throw new ArgumentNullException(nameof(value));
            if (options == null)
                throw new ArgumentNullException(nameof(options));

            var db = _redis.GetDatabase(_database);
            TimeSpan? expiry = GetExpiration(options);
            await db.StringSetAsync(key, value, expiry);
        }

        public void Remove(string key)
        {
            RemoveAsync(key).GetAwaiter().GetResult();
        }

        public async Task RemoveAsync(string key, CancellationToken token = default)
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException(nameof(key));

            var db = _redis.GetDatabase(_database);
            await db.KeyDeleteAsync(key);
        }

        public void Refresh(string key)
        {
            RefreshAsync(key).GetAwaiter().GetResult();
        }

        public async Task RefreshAsync(string key, CancellationToken token = default)
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException(nameof(key));

            var db = _redis.GetDatabase(_database);
            // Extend TTL to 5 minutes (matches ProductController's TTL)
            await db.KeyExpireAsync(key, TimeSpan.FromMinutes(5));
        }

        private TimeSpan? GetExpiration(DistributedCacheEntryOptions options)
        {
            if (options.AbsoluteExpirationRelativeToNow.HasValue)
                return options.AbsoluteExpirationRelativeToNow;
            if (options.AbsoluteExpiration.HasValue)
                return options.AbsoluteExpiration.Value - DateTimeOffset.Now;
            if (options.SlidingExpiration.HasValue)
                return options.SlidingExpiration;
            return null;
        }
    }

}
