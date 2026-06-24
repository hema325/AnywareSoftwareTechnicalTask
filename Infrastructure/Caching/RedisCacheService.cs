using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using System.Text.Json;

namespace Infrastructure.Caching
{
    internal class RedisCacheService : ICache
    {
        private readonly IDatabase _database;
        private readonly ILogger<RedisCacheService> _logger;
        public RedisCacheService(IConnectionMultiplexer connectionMultiplexer, ILogger<RedisCacheService> logger)
        {
            _database = connectionMultiplexer.GetDatabase();
            _logger = logger;
        }

        public async Task<TData> GetAsync<TData>(string key, CancellationToken cancellationToken = default)
        {
            try
            {
                var value = await _database.StringGetAsync(key);

                if (value.IsNullOrEmpty)
                    return default!;

                return JsonSerializer.Deserialize<TData>(value.ToString())!;
            }
            catch (RedisException ex)
            {
                _logger.LogWarning(ex, "Failed to retrieve cache entry with key '{CacheKey}'", key);

                return default;
            }
        }

        public async Task<bool> SetAsync<TData>(string key, TData data, TimeSpan? expiration = null, CancellationToken cancellationToken = default)
        {
            try
            {
                var value = JsonSerializer.Serialize(data);
                return await _database.StringSetAsync(key, value, expiration, When.Always);
            }
            catch (RedisException ex)
            {
                _logger.LogWarning(ex, "Failed to store cache entry with key '{CacheKey}'", key);

                return false;
            }
        }

        public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
        {
            try
            {
                await _database.KeyDeleteAsync(key);
            }
            catch (RedisException ex)
            { 
                _logger.LogWarning(ex, "Failed to store cache entry with key '{CacheKey}'", key);
            }
        }
    }
}
