using System;
using System.Net.Http;

namespace Cyber.Http.Models
{
    /// <summary>
    /// API 动作上下文
    /// </summary>
    public class ApiActionContext
    {
        /// <summary>
        /// HTTP 客户端
        /// </summary>
        public HttpClient HttpClient { get; set; } = null!;

        /// <summary>
        /// HTTP 请求消息
        /// </summary>
        public HttpRequestMessage RequestMessage { get; set; } = null!;

        /// <summary>
        /// 接口类型
        /// </summary>
        public Type InterfaceType { get; set; } = null!;

        /// <summary>
        /// 方法名称
        /// </summary>
        public string MethodName { get; set; } = "";

        /// <summary>
        /// HTTP 方法
        /// </summary>
        public string HttpMethod { get; set; } = "";

        /// <summary>
        /// 路径模板
        /// </summary>
        public string PathTemplate { get; set; } = "";

        /// <summary>
        /// 最终请求 URL
        /// </summary>
        public string RequestUrl { get; set; } = "";

        /// <summary>
        /// 用户自定义数据
        /// </summary>
        public object? UserData { get; set; }

        /// <summary>
        /// 选项配置
        /// </summary>
        public HttpClientOptions Options { get; set; } = null!;
    }

    /// <summary>
    /// API 请求上下文
    /// </summary>
    public class ApiRequestContext
    {
        /// <summary>
        /// HTTP 请求消息
        /// </summary>
        public HttpRequestMessage RequestMessage { get; set; } = null!;

        /// <summary>
        /// 动作上下文
        /// </summary>
        public ApiActionContext ActionContext { get; set; } = null!;
    }

    /// <summary>
    /// API 响应上下文
    /// </summary>
    public class ApiResponseContext
    {
        /// <summary>
        /// HTTP 响应消息
        /// </summary>
        public HttpResponseMessage ResponseMessage { get; set; } = null!;

        /// <summary>
        /// 动作上下文
        /// </summary>
        public ApiActionContext ActionContext { get; set; } = null!;

        /// <summary>
        /// 异常（如果有）
        /// </summary>
        public Exception? Exception { get; set; }
    }
}