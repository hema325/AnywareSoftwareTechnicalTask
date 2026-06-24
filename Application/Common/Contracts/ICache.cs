using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Common.Contracts
{
    public interface ICache
    {
        Task<TData> GetAsync<TData>(string key, CancellationToken cancellationToken = default);
        Task RemoveAsync(string key, CancellationToken cancellationToken = default);
        Task<bool> SetAsync<TData>(string key, TData data, TimeSpan? expiration = null, CancellationToken cancellationToken = default);
    }
}
