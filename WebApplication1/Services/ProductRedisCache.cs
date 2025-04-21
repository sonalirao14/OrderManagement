using StackExchange.Redis;
using Microsoft.Extensions.Caching.Distributed;

namespace WebApplication1.Services
{
    public class ProductRedisCache
    {
        private readonly IConnectionMultiplexer _redis;
        private readonly ILogger<ProductRedisCache> _logger;

        public ProductRedisCache(IConnectionMultiplexer redis, ILogger<ProductRedisCache> logger)
        {
            _redis = redis;
            _logger = logger;
        }

        public async Task InvalidateProductCachesAsync()
        {
            try
            {
                var server = _redis.GetServer(_redis.GetEndPoints()[0]);
                var db = _redis.GetDatabase(); 
                var keys = server.Keys(database: 1, pattern: "product:*");

                foreach (var key in keys)
                {
                    var isDeleted = await db.KeyDeleteAsync(key);
                    if (isDeleted)
                    {
                        _logger.LogInformation("Removed cache key: {key}", key);
                    }
                    else
                    {
                        _logger.LogError("Error in Removing cache key");
                    }
                }

                _logger.LogInformation("Successfully invalidated product cache");
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error while invalidating product cache");
            }
        }
    }
}
