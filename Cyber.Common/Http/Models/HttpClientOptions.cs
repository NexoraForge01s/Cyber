using System;
using System.Collections.Generic;

namespace Cyber.Http.Models
{
    /// <summary>
    /// HTTP 客户端配置选项
    /// </summary>
    public class HttpClientOptions
    {
        /// <summary>
        /// HTTP 基地址
        /// </summary>
        public Uri? HttpHost { get; set; }

        /// <summary>
        /// 全局过滤器列表
        /// </summary>
        public List<IApiActionFilter> GlobalFilters { get; } = new();

        /// <summary>
        /// 默认请求头
        /// </summary>
        public Dictionary<string, string> DefaultHeaders { get; } = new();

        /// <summary>
        /// 超时时间（秒）
        /// </summary>
        public int TimeoutSeconds { get; set; } = 100;
    }
}