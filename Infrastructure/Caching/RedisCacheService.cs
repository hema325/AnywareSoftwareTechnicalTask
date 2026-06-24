using StackExchange.Redis;
using System.Text.Json;

namespace Infrastructure.Caching
{
    internal class RedisCacheService: ICache
    {
        private readonly IDatabase _database;
        public RedisCacheService(IConnectionMultiplexer connectionMultiplexer)
        {
            _database = connectionMultiplexer.GetDatabase();
        }

        public async Task<TData> GetAsync<TData>(string key, CancellationToken cancellationToken = default)
        {
            var value = await _database.StringGetAsync(key);
            
            if (value.IsNullOrEmpty)
                return default!;

            return JsonSerializer.Deserialize<TData>(value.ToString())!;
        }

        public async Task<bool> SetAsync<TData>(string key, TData data, TimeSpan? expiration = null, CancellationToken cancellationToken = default)
        {
            var value = JsonSerializer.Serialize(data);
            return await _database.StringSetAsync(key, value, expiration, When.Always);
        }

        public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
        {
            await _database.KeyDeleteAsync(key);
        }
    }
}
