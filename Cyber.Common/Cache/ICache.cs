using System;

namespace Cyber
{
    /// <summary>
    /// 缓存接口，用于重复请求拦截等场景
    /// </summary>
    public interface ICache
    {
        /// <summary>
        /// 获取缓存值
        /// </summary>
        bool TryGetValue(string key, out string? value);

        /// <summary>
        /// 设置缓存值及过期时间
        /// </summary>
        void Set(string key, string value, int expirationSeconds);

        /// <summary>
        /// 删除缓存
        /// </summary>
        void Del(string key);
    }
}
