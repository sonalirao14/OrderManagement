using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace Infrastructure.Caching
{
    public interface IRedisConnectionFactory
    {
        IConnectionMultiplexer GetConnection();
    }

    public class RedisConnectionFactory : IRedisConnectionFactory
    {
        private readonly RedisConfiguration _config;
        private readonly ILogger<RedisConnectionFactory> _logger;
        //private readonly ILoggingService _logger;
        private readonly Lazy<IConnectionMultiplexer> _connection;

        public RedisConnectionFactory(IOptions<RedisConfiguration> configOptions, ILogger<RedisConnectionFactory> logger)
        {
            _config = configOptions.Value;
            _logger = logger;
            _connection = new Lazy<IConnectionMultiplexer>(() => CreateConnection());
        }

        public IConnectionMultiplexer GetConnection() => _connection.Value;

        private IConnectionMultiplexer CreateConnection()
        {
            try
            {
                var config = new ConfigurationOptions
                {
                    EndPoints = { _config.Host },
                    DefaultDatabase = _config.DefaultDatabase,
                    ConnectTimeout = _config.ConnectTimeout,
                    AbortOnConnectFail = _config.AbortOnConnectFail
                };

                _logger.LogInformation("Connecting to Redis at: {RedisHost}", _config.Host);
                return ConnectionMultiplexer.Connect(config);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to connect to Redis at {RedisHost}.", _config.Host);
                throw;
            }
        }
    }
}
