using System;

namespace Cyber
{
    /// <summary>
    /// 标记一个类为 MiniAPI 控制器，源生成器会为其公共方法自动生成 Minimal API 端点映射。
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public sealed class MiniApi : Attribute
    {
        /// <summary>
        /// 路由前缀，默认为 null 时根据类名自动推断。
        /// 例如 "api/user" 会生成 /api/user/xxx 的路由。
        /// </summary>
        public string? RoutePrefix { get; set; }

        /// <summary>
        /// 标签名称，用于 Swagger 分组。默认为 null 时使用类名（去除后缀）。
        /// </summary>
        public string? Tag { get; set; }
    }

    /// <summary>
    /// 标记一个方法为 MiniAPI 端点，可自定义路由模板和 HTTP 方法。
    /// 如果未标记，则根据方法名自动推断。
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public sealed class MiniApiMethod : Attribute
    {
        /// <summary>
        /// HTTP 方法，如 "GET"、"POST"、"PUT"、"DELETE"、"PATCH"。
        /// 为 null 时根据方法名自动推断。
        /// </summary>
        public string? HttpMethod { get; set; }

        /// <summary>
        /// 自定义路由模板，如 "{id}"。
        /// 为 null 时根据方法名自动推断。
        /// </summary>
        public string? RouteTemplate { get; set; }
    }

    /// <summary>
    /// 为 MiniAPI 端点添加自定义过滤器。
    /// 可标记在类上（应用于该类所有端点）或方法上（仅应用于该方法）。
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = false)]
    public sealed class MiniApiFilter : Attribute
    {
        /// <summary>
        /// 过滤器类型，必须实现 IEndpointFilter 接口。
        /// </summary>
        public Type FilterType { get; }

        public MiniApiFilter(Type filterType)
        {
            FilterType = filterType;
        }
    }

    /// <summary>
    /// 标记 MiniAPI 方法参数来源为路由。
    /// 默认情况下，简单类型从路由/查询绑定，复杂类型从 Body 绑定。
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = false)]
    public sealed class FromRoute : Attribute
    {
    }

    /// <summary>
    /// 标记 MiniAPI 方法参数来源为查询字符串。
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = false)]
    public sealed class FromQuery : Attribute
    {
    }

    /// <summary>
    /// 标记 MiniAPI 方法参数来源为请求体。
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = false)]
    public sealed class FromBody : Attribute
    {
    }

    /// <summary>
    /// 标记 MiniAPI 方法参数来源为表单字段（application/x-www-form-urlencoded 或 multipart/form-data）。
    /// 简单类型使用 Form-data 字段；文件类型（IFormFile）使用 Form-data 文件。
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = false)]
    public sealed class FromForm : Attribute
    {
    }
}
