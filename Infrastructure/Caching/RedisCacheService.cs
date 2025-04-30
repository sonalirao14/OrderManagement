using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using Infrastructure.Caching;

using Core.Interfaces;

namespace Infrastructure.Caching
{
    public class RedisCacheService<T> : ICacheService<T> where T : class
    {
        private readonly IRedisConnectionFactory _redisFactory;
        private readonly ILogger<RedisCacheService<T>> _logger;
        //private readonly ILoggingService _logger;
        private readonly IDatabase _database;

        public RedisCacheService(IRedisConnectionFactory redisFactory, ILogger<RedisCacheService<T>> logger)
        {
            _redisFactory = redisFactory;
            _logger = logger;
            _database = _redisFactory.GetConnection().GetDatabase();
        }

        public async Task<T> GetAsync(string key)
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException(nameof(key));

            try
            {
                var value = await _database.StringGetAsync(key);
                if (!value.HasValue)
                    return null;

                return JsonSerializer.Deserialize<T>(value);
            }
            catch (Exception ex)
            {
                //await _logger.LogErrorAsync(ex, "Error retrieving key {Key} from Redis.", key);
                _logger.LogError(ex,"Error retrieving key {key} from redis", key);
                throw;
            }
        }

        public async Task SetAsync(string key, T value, TimeSpan expirationTime)
        {
            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentNullException(nameof(key));
                //ArgumentException.ThrowIfNullOrEmpty(nameof(key));
            }
            if (value == null)
                throw new ArgumentNullException(nameof(value));
            //ArgumentException.ThrowIfNullOrEmpty(nameof(value)); 

            try
            {
                var serialized = JsonSerializer.Serialize(value);
                await _database.StringSetAsync(key, serialized, expirationTime);
            }
            catch (Exception ex)
            {
                //await _logger.LogErrorAsync(ex, "Error setting key {Key} in Redis.", key);
                _logger.LogError(ex, "Error setting key {key} in Redis", key);
                throw;
            }
        }

        public async Task<bool> SetIfNotExistsAsync(string key, T value, TimeSpan expirationTime)
        {
            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentNullException(nameof(key));
            }
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }
            try
            {
                var serializedVal = JsonSerializer.Serialize(value);
                return await _database.StringSetAsync(key, serializedVal, expirationTime, when: When.NotExists);
            }
            catch (Exception ex)
            {
                //await _logger.LogErrorAsync(ex, "Error setting key if this not exists in Redis!!", key);
                _logger.LogError(ex, "Error while setting key {key}" , key);
                throw;
            }
        }

        public async Task RemoveAsync(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentNullException(nameof(key));
            }

            try
            {
                await _database.KeyDeleteAsync(key);
            }
            catch (Exception ex)
            {
                //await _logger.LogErrorAsync(ex, "Error removing key from Redis.", key);
                _logger.LogError(ex, "Error while removing key {key} from redis", key);
                throw;
            }
        }

        public async Task<bool> KeyExistsAsync(string key)
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException(nameof(key));

            try
            {
                return await _database.KeyExistsAsync(key);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error retrieving key {key} from redis", key);
                throw;
            }
        }

        public async Task InvalidateByPatternAsync(string pattern)
        {
            try
            {
                var server = _redisFactory.GetConnection().GetServer(_redisFactory.GetConnection().GetEndPoints()[0]);
                var keys = server.Keys(database: _database.Database, pattern: pattern);

                foreach (var key in keys)
                {
                    await _database.KeyDeleteAsync(key);
                    //await   _logger.LogInformationAsync("Removed cache key by pattern:", key);
                    _logger.LogInformation("Removed cache key{key} by patttern: ", key);
                }

                //await _logger.LogInformationAsync("Successfully invalidated keys with pattern:", pattern);
                _logger.LogInformation("Successfully invalidated keys with pattern: ", pattern);
            }
            catch (Exception ex)
            {
                //await  _logger.LogErrorAsync(ex, "Error invalidating keys with pattern", pattern);    
                _logger.LogError(ex, "Error invalidating keys {keys} with pattern", pattern);
                throw;
            }
        }


        public async Task ClearAsync()
        {
            try
            {
                var server = _redisFactory.GetConnection().GetServer(_redisFactory.GetConnection().GetEndPoints()[0]);
                await server.FlushDatabaseAsync(_database.Database);
            }
            catch (Exception ex)
            {
                //await _logger.LogErrorAsync(ex, "Error clearing Redis database.");
                _logger.LogError(ex, "Error clearing Redis db..");
                throw;
            }
        }
    }
}
