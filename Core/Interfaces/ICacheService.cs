using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Interfaces
{

    public interface ICacheService<T> where T : class
    {
        Task<T> GetAsync(string key);
        Task SetAsync(string key, T value, TimeSpan expirationTime);
        Task<bool> SetIfNotExistsAsync(string key, T value, TimeSpan expirationTime);
        Task RemoveAsync(string key);
        Task InvalidateByPatternAsync(string pattern);
        Task<bool> KeyExistsAsync(string key);
        Task ClearAsync();
    }
}
