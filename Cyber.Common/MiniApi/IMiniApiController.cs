namespace Cyber
{
    /// <summary>
    /// 标记一个类为 MiniAPI 控制器。源生成器会为实现了此接口的类的公共方法自动生成 Minimal API 端点映射。
    /// 配合 [Route] 特性配置路由前缀，配合 [HttpGet/HttpPost/等] 特性配置 HTTP 方法和路由模板。
    /// </summary>
    public interface IMiniApiController
    {
    }
}