using System;
using System.Collections.Concurrent;
using System.Threading;

namespace Cyber
{
    /// <summary>
    /// 基于 Memory 的默认 ICache 实现
    /// </summary>
    public class MemoryCache : ICache
    {
        private readonly ConcurrentDictionary<string, CacheEntry> _cache = new();

        public bool TryGetValue(string key, out string? value)
        {
            if (_cache.TryGetValue(key, out var entry))
            {
                if (entry.Expiration > DateTime.UtcNow)
                {
                    value = entry.Value;
                    return true;
                }
                // 已过期，移除
                _cache.TryRemove(key, out _);
            }
            value = null;
            return false;
        }

        public void Set(string key, string value, int expirationSeconds)
        {
            var entry = new CacheEntry(value, DateTime.UtcNow.AddSeconds(expirationSeconds));
            _cache.AddOrUpdate(key, entry, (_, __) => entry);
        }

        public void Del(string key)
        {
            _cache.TryRemove(key, out _);
        }

        private readonly record struct CacheEntry(string Value, DateTime Expiration);
    }
}
