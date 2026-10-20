using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;

namespace Cyber.Http.Extensions
{
    /// <summary>
    /// HttpRequestMessage 扩展方法
    /// </summary>
    public static class HttpRequestMessageExtensions
    {
        /// <summary>
        /// 添加 URL 查询参数
        /// </summary>
        public static void AddUrlQuery(this HttpRequestMessage request, string name, string value)
        {
            var uri = request.RequestUri;
            if (uri == null) return;

            var query = uri.Query;
            var builder = new StringBuilder();

            if (query.StartsWith("?"))
            {
                builder.Append(query.Substring(1));
                if (builder.Length > 0)
                {
                    builder.Append("&");
                }
            }

            builder.Append($"{Uri.EscapeDataString(name)}={Uri.EscapeDataString(value)}");

            var newUri = new UriBuilder(uri)
            {
                Query = builder.ToString()
            }.Uri;

            request.RequestUri = newUri;
        }

        /// <summary>
        /// 添加多个 URL 查询参数
        /// </summary>
        public static void AddUrlQueries(this HttpRequestMessage request, IEnumerable<KeyValuePair<string, string>> queries)
        {
            foreach (var kv in queries)
            {
                request.AddUrlQuery(kv.Key, kv.Value);
            }
        }

        /// <summary>
        /// 尝试获取响应（用于过滤器）
        /// </summary>
        public static HttpResponseMessage? GetResponseOrNull(this HttpRequestMessage request)
        {
            // 在实际场景中，响应会存储在上下文中
            // 这里提供一个占位实现
            return null;
        }
    }
}